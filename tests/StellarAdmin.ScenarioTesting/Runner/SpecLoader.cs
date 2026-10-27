using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.ScenarioTesting.Scenarios;
using YamlDotNet.RepresentationModel;

namespace StellarAdmin.ScenarioTesting.Runner;

// Reads a .spec.yaml file into steps; each step is a single-key mapping such as "- goto: /stellaradmin"
public static class SpecLoader
{
    // Defines window.specAnnotations, which annotate and spotlight draw with; safe to run again
    private static readonly string AnnotationsScript = ReadAnnotationsScript();

    // Milliseconds an assertion or click retries before failing; a step can set its own timeout
    private const float DefaultTimeout = 1000;

    // Errors name the file by displayPath, e.g. Specs/branding.spec.yaml; defaults to the file
    public static Spec Load(string file, string? displayPath = null)
    {
        var path = displayPath ?? file;
        var stream = new YamlStream();
        using (var reader = File.OpenText(file))
        {
            stream.Load(reader);
        }

        if (stream.Documents is not [{ RootNode: YamlMappingNode root }])
        {
            throw new SpecException(path, 1, "A spec must be a single YAML mapping.");
        }

        var scenarioName = Scalar(path, Child(path, root, "scenario"));
        var scenario =
            ScenarioCatalog.Find(scenarioName)
            ?? throw new SpecException(path, Line(root), $"Unknown scenario '{scenarioName}'.");

        if (Child(path, root, "steps") is not YamlSequenceNode steps)
        {
            throw new SpecException(path, Line(root), "'steps' must be a list.");
        }

        List<SpecStep> loaded = [.. steps.Select(step => Step(path, step))];

        // A top-level user signs in before the first step
        if (root.Children.TryGetValue(new YamlScalarNode("user"), out var user))
        {
            loaded.Insert(0, new("signIn", Line(user), SignIn(Scalar(path, user))));
        }

        CheckRecordings(path, loaded);

        return new(path, scenario, loaded);
    }

    // A record runs until recordEnd or the end of the spec; clips cannot overlap
    private static void CheckRecordings(string path, IEnumerable<SpecStep> steps)
    {
        int? open = null;
        foreach (var step in steps)
        {
            if (step.Kind == "record")
            {
                if (open is not null)
                {
                    throw new SpecException(
                        path,
                        step.Line,
                        $"The recording started on line {open} has not ended."
                    );
                }

                open = step.Line;
            }
            else if (step.Kind == "recordEnd")
            {
                if (open is null)
                {
                    throw new SpecException(path, step.Line, "recordEnd needs a record before it.");
                }

                open = null;
            }
        }
    }

    private static YamlNode Child(string path, YamlMappingNode mapping, string key) =>
        mapping.Children.TryGetValue(new YamlScalarNode(key), out var value)
            ? value
            : throw new SpecException(path, Line(mapping), $"Missing '{key}'.");

    private static int Line(YamlNode node) => (int)node.Start.Line;

    // A string is a CSS selector; a mapping has either selector, or role with an optional name. Inside
    // within, both search the scoped element, and a mapping with neither is the scoped element itself
    private static Func<SpecRun, ILocator> Locator(string path, YamlNode node)
    {
        if (node is YamlScalarNode)
        {
            var selector = Scalar(path, node);

            return run => run.Scope?.Locator(selector) ?? run.Page.Locator(selector);
        }

        if (node is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(node), "Expected a selector or a locator mapping.");
        }

        if (OptionalScalar(path, mapping, "selector") is { } css)
        {
            return run => run.Scope?.Locator(css) ?? run.Page.Locator(css);
        }

        if (OptionalScalar(path, mapping, "role") is not { } roleName)
        {
            var line = Line(node);

            return run =>
                run.Scope
                ?? throw new SpecException(
                    path,
                    line,
                    "A locator needs 'selector' or 'role' outside within."
                );
        }

        if (!Enum.TryParse<AriaRole>(roleName, ignoreCase: true, out var role))
        {
            throw new SpecException(path, Line(node), $"Unknown ARIA role '{roleName}'.");
        }

        var name = OptionalScalar(path, mapping, "name");
        var exact = name is not null;

        return run =>
            run.Scope?.GetByRole(role, new() { Name = name, Exact = exact })
            ?? run.Page.GetByRole(role, new() { Name = name, Exact = exact });
    }

    // An optional value that must be one of the allowed words
    private static string? OneOf(
        string path,
        YamlMappingNode mapping,
        string key,
        params string[] allowed
    )
    {
        var value = OptionalScalar(path, mapping, key);
        if (value is not null && !allowed.Contains(value))
        {
            throw new SpecException(
                path,
                Line(mapping.Children[new YamlScalarNode(key)]),
                $"'{key}' must be one of {string.Join(", ", allowed)}."
            );
        }

        return value;
    }

    private static string? OptionalScalar(string path, YamlMappingNode mapping, string key) =>
        mapping.Children.TryGetValue(new YamlScalarNode(key), out var value)
            ? Scalar(path, value)
            : null;

    private static string ReadAnnotationsScript()
    {
        using var stream = typeof(SpecLoader).Assembly.GetManifestResourceStream("annotations.js")!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private static string Scalar(string path, YamlNode node) =>
        node is YamlScalarNode { Value: { } value }
            ? value
            : throw new SpecException(path, Line(node), "Expected a single value.");

    // Text to match exactly, or /pattern/ for a regular expression
    private static (string? Exact, Regex? Pattern) Text(string path, YamlNode node)
    {
        var text = Scalar(path, node);

        return text is ['/', .. var pattern, '/'] ? (null, new Regex(pattern)) : (text, null);
    }

    private static float Speed(string path, YamlNode node) =>
        float.TryParse(
            Scalar(path, node),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var speed
        )
        && speed > 0
            ? speed
            : throw new SpecException(path, Line(node), "'cursorSpeed' must be a positive number.");

    private static SpecStep Step(string path, YamlNode node)
    {
        if (node is not YamlMappingNode { Children.Count: 1 } step)
        {
            throw new SpecException(
                path,
                Line(node),
                "A step must be a single 'kind: value' pair."
            );
        }

        var entry = step.Children.Single();
        var kind = Scalar(path, entry.Key);
        var value = entry.Value;
        var timeout = Timeout(path, value);
        Func<SpecRun, Task> run = kind switch
        {
            "annotate" => Annotate(path, value, timeout),
            "clearAnnotations" => ClearAnnotations,
            "check" => Pointer(
                Locator(path, value),
                timeout,
                l => l.CheckAsync(new() { Timeout = timeout })
            ),
            "click" => Pointer(
                Locator(path, value),
                timeout,
                l => l.ClickAsync(new() { Timeout = timeout })
            ),
            "colorScheme" => ColorScheme(path, value),
            "expectAttribute" => ExpectAttribute(path, value, timeout),
            "expectCount" => ExpectCount(path, value, timeout),
            "expectText" => ExpectText(path, value, timeout),
            "expectTitle" => ExpectTitle(path, value, timeout),
            "expectUrl" => ExpectUrl(Url(path, value), timeout),
            "expectVisible" => ExpectVisible(Locator(path, value), timeout),
            "fill" => Fill(path, value, timeout),
            "goto" => Goto(path, Line(node), Scalar(path, value)),
            "hover" => Pointer(
                Locator(path, value),
                timeout,
                l => l.HoverAsync(new() { Timeout = timeout })
            ),
            "pause" => Pause(path, value),
            "press" => Press(Scalar(path, value)),
            "record" => Record(path, Line(node), value),
            "recordEnd" => RecordEnd,
            "screenshot" => Screenshot(path, value, timeout),
            "scroll" => Scroll(Locator(path, value), timeout),
            "select" => Select(path, value, timeout),
            "signIn" => SignIn(Scalar(path, value)),
            "spotlight" => Spotlight(Locator(path, value), timeout),
            "type" => Type(path, value, timeout),
            "uncheck" => Pointer(
                Locator(path, value),
                timeout,
                l => l.UncheckAsync(new() { Timeout = timeout })
            ),
            "viewport" => Viewport(path, value),
            "within" => Within(path, value),
            _ => throw new SpecException(path, Line(node), $"Unknown step '{kind}'."),
        };

        return new(kind, Line(node), run);
    }

    // A mapping value can set "timeout" in milliseconds, e.g. { role: dialog, timeout: 3000 }
    private static float Timeout(string path, YamlNode value)
    {
        if (
            value is not YamlMappingNode mapping
            || OptionalScalar(path, mapping, "timeout") is not { } text
        )
        {
            return DefaultTimeout;
        }

        if (
            !float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var timeout)
            || timeout <= 0
        )
        {
            throw new SpecException(
                path,
                Line(value),
                "'timeout' must be a positive number of milliseconds."
            );
        }

        return timeout;
    }

    // A path, or { url, timeout } to change the timeout
    private static string Url(string path, YamlNode value) =>
        value is YamlMappingNode mapping
            ? Scalar(path, Child(path, mapping, "url"))
            : Scalar(path, value);

    // { locator, style, badge, label, placement, arrow, gap }; the overlays stay until clearAnnotations,
    // so a screenshot after this step includes them. badge: auto numbers on from the badges already shown
    private static Func<SpecRun, Task> Annotate(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "annotate needs a 'locator'.");
        }

        var locate = Locator(path, Child(path, mapping, "locator"));
        var options = new
        {
            style = OneOf(
                path,
                mapping,
                "style",
                "box",
                "highlight",
                "circle",
                "underline",
                "none"
            ),
            badge = OptionalScalar(path, mapping, "badge"),
            label = OptionalScalar(path, mapping, "label"),
            placement = OneOf(path, mapping, "placement", "top", "right", "bottom", "left"),
            arrow = OneOf(path, mapping, "arrow", "true", "false") == "true",
            gap = mapping.Children.ContainsKey(new YamlScalarNode("gap"))
                ? Pixels(path, mapping, "gap")
                : (int?)null,
        };

        return async run =>
        {
            await run.Page.EvaluateAsync(AnnotationsScript);
            await locate(run)
                .EvaluateAsync(
                    "(element, options) => window.specAnnotations.annotate(element, options)",
                    options,
                    new() { Timeout = timeout }
                );
        };
    }

    private static Task ClearAnnotations(SpecRun run) =>
        run.Page.EvaluateAsync(
            "document.querySelectorAll('[data-spec-annotation]').forEach(overlay => overlay.remove())"
        );

    // dark or light; sets the class the stylesheets follow on <html> without reloading the page
    private static Func<SpecRun, Task> ColorScheme(string path, YamlNode value)
    {
        var scheme = Scalar(path, value);
        if (scheme is not ("dark" or "light"))
        {
            throw new SpecException(path, Line(value), "'colorScheme' must be dark or light.");
        }

        var other = scheme == "dark" ? "light" : "dark";

        return run =>
            run.Page.EvaluateAsync(
                $"document.documentElement.classList.remove('{other}'); document.documentElement.classList.add('{scheme}')"
            );
    }

    // A locator mapping plus the attribute and its value, e.g. { selector: "a.brand", attribute: href,
    // value: /home }; the element need not be visible
    private static Func<SpecRun, Task> ExpectAttribute(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(
                path,
                Line(value),
                "expectAttribute needs a locator, 'attribute' and 'value'."
            );
        }

        var locate = Locator(path, mapping);
        var attribute = Scalar(path, Child(path, mapping, "attribute"));
        var (exact, pattern) = Text(path, Child(path, mapping, "value"));

        return run =>
            pattern is null
                ? Assertions
                    .Expect(locate(run))
                    .ToHaveAttributeAsync(attribute, exact!, new() { Timeout = timeout })
                : Assertions
                    .Expect(locate(run))
                    .ToHaveAttributeAsync(attribute, pattern, new() { Timeout = timeout });
    }

    // A locator mapping plus how many elements it matches, e.g. { selector: ".subtitle", count: 0 }
    private static Func<SpecRun, Task> ExpectCount(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "expectCount needs a locator and 'count'.");
        }

        var locate = Locator(path, mapping);
        var countNode = Child(path, mapping, "count");
        if (
            !int.TryParse(
                Scalar(path, countNode),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var count
            )
        )
        {
            throw new SpecException(path, Line(countNode), "'count' must be a whole number.");
        }

        return run =>
            Assertions.Expect(locate(run)).ToHaveCountAsync(count, new() { Timeout = timeout });
    }

    // A locator mapping plus the text, e.g. { role: heading, text: Trips }
    private static Func<SpecRun, Task> ExpectText(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "expectText needs a locator and 'text'.");
        }

        var locate = Locator(path, mapping);
        var (exact, pattern) = Text(path, Child(path, mapping, "text"));

        return run =>
            pattern is null
                ? Assertions
                    .Expect(locate(run))
                    .ToHaveTextAsync(exact!, new() { Timeout = timeout })
                : Assertions
                    .Expect(locate(run))
                    .ToHaveTextAsync(pattern, new() { Timeout = timeout });
    }

    // The document title, or { text, timeout }
    private static Func<SpecRun, Task> ExpectTitle(string path, YamlNode value, float timeout)
    {
        var (exact, pattern) = Text(
            path,
            value is YamlMappingNode mapping ? Child(path, mapping, "text") : value
        );

        return run =>
            pattern is null
                ? Assertions.Expect(run.Page).ToHaveTitleAsync(exact!, new() { Timeout = timeout })
                : Assertions
                    .Expect(run.Page)
                    .ToHaveTitleAsync(pattern, new() { Timeout = timeout });
    }

    // { locator, text } replaces the element's value at once; type types key by key instead
    private static Func<SpecRun, Task> Fill(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "fill needs a 'locator' and 'text'.");
        }

        var locate = Locator(path, Child(path, mapping, "locator"));
        var text = Scalar(path, Child(path, mapping, "text"));

        return Pointer(
            locate,
            timeout,
            locator => locator.FillAsync(text, new() { Timeout = timeout })
        );
    }

    // Matches the end of the URL, so specs only need the path
    private static Func<SpecRun, Task> ExpectUrl(string url, float timeout) =>
        run =>
            Assertions
                .Expect(run.Page)
                .ToHaveURLAsync(new Regex($"{Regex.Escape(url)}$"), new() { Timeout = timeout });

    private static Func<SpecRun, Task> ExpectVisible(
        Func<SpecRun, ILocator> locate,
        float timeout
    ) => run => Assertions.Expect(locate(run)).ToBeVisibleAsync(new() { Timeout = timeout });

    private static Func<SpecRun, Task> Goto(string path, int line, string url) =>
        async run =>
        {
            var response = await run.Page.GotoAsync(url);
            if (response is not { Ok: true })
            {
                throw new SpecException(path, line, $"{url} returned {response?.Status}.");
            }
        };

    // While recording, the cursor glides to the element before click, hover, check, uncheck, fill and
    // select act on it
    private static Func<SpecRun, Task> Pointer(
        Func<SpecRun, ILocator> locate,
        float timeout,
        Func<ILocator, Task> act
    ) =>
        async run =>
        {
            var locator = locate(run);
            if (run.Recording is not null)
            {
                await run.Video!.MoveToAsync(locator, timeout);
            }

            await act(locator);
        };

    // Milliseconds to wait while recording, so a result stays on screen; does nothing otherwise
    private static Func<SpecRun, Task> Pause(string path, YamlNode value)
    {
        var milliseconds = Milliseconds(path, value, "pause");

        return run =>
            run.Recording is null ? Task.CompletedTask : run.Page.WaitForTimeoutAsync(milliseconds);
    }

    // A key or chord on the focused element, e.g. Enter or Control+K
    private static Func<SpecRun, Task> Press(string key) =>
        run => run.Page.Keyboard.PressAsync(key, new() { Delay = run.Recording is null ? 0 : 60 });

    // A clip name, or { name, gif, typeDelay, cursorSpeed, hold }: records the following steps until
    // recordEnd as {name}.mp4, plus {name}.gif with gif: true. typeDelay is milliseconds per key (90),
    // cursorSpeed scales the cursor's glide (1) and hold keeps the last frame for milliseconds (800).
    // Recording happens only when screenshots are saved
    private static Func<SpecRun, Task> Record(string path, int line, YamlNode value)
    {
        var options = value is YamlMappingNode mapping
            ? new SpecRecordOptions(
                Scalar(path, Child(path, mapping, "name")),
                line,
                OneOf(path, mapping, "gif", "true", "false") == "true",
                mapping.Children.TryGetValue(new YamlScalarNode("typeDelay"), out var typeDelay)
                    ? Milliseconds(path, typeDelay, "typeDelay")
                    : 90,
                mapping.Children.TryGetValue(new YamlScalarNode("cursorSpeed"), out var speed)
                    ? Speed(path, speed)
                    : 1,
                mapping.Children.TryGetValue(new YamlScalarNode("hold"), out var hold)
                    ? Milliseconds(path, hold, "hold")
                    : 800
            )
            : new SpecRecordOptions(Scalar(path, value), line, false, 90, 1, 800);

        return run => run.Video?.BeginAsync(run.Page, options) ?? Task.CompletedTask;
    }

    private static Task RecordEnd(SpecRun run) =>
        run.Recording is null ? Task.CompletedTask : run.Video!.EndAsync(run.Page);

    // A name captures the viewport; { name, locator } captures only that element, e.g.
    // { name: brand, locator: ".sa-sidebar-brand" }
    private static Func<SpecRun, Task> Screenshot(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            var name = Scalar(path, value);

            return run =>
                run.ScreenshotDirectory is null
                    ? Task.CompletedTask
                    : run.Page.ScreenshotAsync(
                        new()
                        {
                            Animations = ScreenshotAnimations.Disabled,
                            Path = Path.Combine(run.ScreenshotDirectory, $"{name}.png"),
                        }
                    );
        }

        var elementName = Scalar(path, Child(path, mapping, "name"));
        var locate = Locator(path, Child(path, mapping, "locator"));

        return run =>
            run.ScreenshotDirectory is null
                ? Task.CompletedTask
                : locate(run)
                    .ScreenshotAsync(
                        new()
                        {
                            Animations = ScreenshotAnimations.Disabled,
                            Path = Path.Combine(run.ScreenshotDirectory, $"{elementName}.png"),
                            Timeout = timeout,
                        }
                    );
    }

    // Scrolls the element into view; while recording, smoothly
    private static Func<SpecRun, Task> Scroll(Func<SpecRun, ILocator> locate, float timeout) =>
        run =>
            run.Recording is null
                ? locate(run).ScrollIntoViewIfNeededAsync(new() { Timeout = timeout })
                : SpecVideo.ScrollToAsync(locate(run), timeout);

    // { locator, option } picks the option of a <select> by its value or label
    private static Func<SpecRun, Task> Select(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "select needs a 'locator' and 'option'.");
        }

        var locate = Locator(path, Child(path, mapping, "locator"));
        var option = Scalar(path, Child(path, mapping, "option"));

        return Pointer(
            locate,
            timeout,
            locator => locator.SelectOptionAsync(option, new() { Timeout = timeout })
        );
    }

    // Signs in as the seeded user with this email through the scenario's sign-in page; the request shares the
    // browser's cookies, so the next page loads signed in
    private static Func<SpecRun, Task> SignIn(string email) =>
        async run =>
        {
            var response = await run.Page.Context.APIRequest.GetAsync(
                $"{SignInSetup.SignInPath}?email={Uri.EscapeDataString(email)}",
                new() { MaxRedirects = 0 }
            );
            if (response.Status != 302)
            {
                throw new InvalidOperationException($"No seeded user has the email '{email}'.");
            }
        };

    // Dims the page except for every element the locator matches; annotations still draw on top
    private static Func<SpecRun, Task> Spotlight(Func<SpecRun, ILocator> locate, float timeout) =>
        async run =>
        {
            var locator = locate(run);
            await locator.First.WaitForAsync(new() { Timeout = timeout });
            await run.Page.EvaluateAsync(AnnotationsScript);
            await locator.EvaluateAllAsync<object?>(
                "elements => window.specAnnotations.spotlight(elements)"
            );
        };

    // { locator, text } types into the element key by key; while recording, at the clip's typeDelay
    private static Func<SpecRun, Task> Type(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "type needs a 'locator' and 'text'.");
        }

        var locate = Locator(path, Child(path, mapping, "locator"));
        var text = Scalar(path, Child(path, mapping, "text"));

        return run =>
            locate(run)
                .PressSequentiallyAsync(
                    text,
                    new() { Delay = run.Recording?.TypeDelay ?? 0, Timeout = timeout }
                );
    }

    // Resizes the page without reloading it, e.g. { width: 390, height: 844 }
    private static Func<SpecRun, Task> Viewport(string path, YamlNode value)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "viewport needs 'width' and 'height'.");
        }

        var width = Pixels(path, mapping, "width");
        var height = Pixels(path, mapping, "height");

        return run => run.Page.SetViewportSizeAsync(width, height);
    }

    // { <locator>, steps }: runs the steps with their locators searching inside the element, e.g.
    // { selector: "[data-slot='sidebar-header']", steps: [...] }; within steps can nest
    private static Func<SpecRun, Task> Within(string path, YamlNode value)
    {
        if (
            value is not YamlMappingNode mapping
            || Child(path, mapping, "steps") is not YamlSequenceNode steps
        )
        {
            throw new SpecException(
                path,
                Line(value),
                "within needs a locator and a 'steps' list."
            );
        }

        var locate = Locator(path, mapping);
        List<SpecStep> loaded = [.. steps.Select(step => Step(path, step))];
        if (loaded.FirstOrDefault(step => step.Kind is "record" or "recordEnd") is { } recording)
        {
            throw new SpecException(
                path,
                recording.Line,
                $"{recording.Kind} can't be inside within."
            );
        }

        return async run =>
        {
            var scoped = run with { Scope = locate(run) };
            foreach (var step in loaded)
            {
                try
                {
                    await step.RunAsync(scoped);
                }
                catch (Exception exception) when (exception is not SpecException)
                {
                    throw new SpecException(
                        path,
                        step.Line,
                        $"{step.Kind}: {exception.Message}",
                        exception
                    );
                }
            }
        };
    }

    private static int Milliseconds(string path, YamlNode node, string key) =>
        int.TryParse(
            Scalar(path, node),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var milliseconds
        )
            ? milliseconds
            : throw new SpecException(path, Line(node), $"'{key}' must be whole milliseconds.");

    private static int Pixels(string path, YamlMappingNode mapping, string key)
    {
        var node = Child(path, mapping, key);
        if (
            !int.TryParse(
                Scalar(path, node),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var pixels
            )
            || pixels <= 0
        )
        {
            throw new SpecException(
                path,
                Line(node),
                $"'{key}' must be a positive whole number of pixels."
            );
        }

        return pixels;
    }
}
