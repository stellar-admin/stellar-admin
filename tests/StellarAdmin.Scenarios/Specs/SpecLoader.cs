using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using YamlDotNet.RepresentationModel;

namespace StellarAdmin.Scenarios.Specs;

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

    // A string is a CSS selector; a mapping has either selector, or role with an optional name
    private static Func<IPage, ILocator> Locator(string path, YamlNode node)
    {
        if (node is YamlScalarNode)
        {
            var selector = Scalar(path, node);

            return page => page.Locator(selector);
        }

        if (node is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(node), "Expected a selector or a locator mapping.");
        }

        if (OptionalScalar(path, mapping, "selector") is { } css)
        {
            return page => page.Locator(css);
        }

        var roleName =
            OptionalScalar(path, mapping, "role")
            ?? throw new SpecException(path, Line(node), "A locator needs 'selector' or 'role'.");
        if (!Enum.TryParse<AriaRole>(roleName, ignoreCase: true, out var role))
        {
            throw new SpecException(path, Line(node), $"Unknown ARIA role '{roleName}'.");
        }

        var name = OptionalScalar(path, mapping, "name");

        return page => page.GetByRole(role, new() { Name = name, Exact = name is not null });
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
            "click" => Click(Locator(path, value), timeout),
            "colorScheme" => ColorScheme(path, value),
            "expectText" => ExpectText(path, value, timeout),
            "expectUrl" => ExpectUrl(Url(path, value), timeout),
            "expectVisible" => ExpectVisible(Locator(path, value), timeout),
            "goto" => Goto(path, Line(node), Scalar(path, value)),
            "pause" => Pause(path, value),
            "press" => Press(Scalar(path, value)),
            "record" => Record(path, Line(node), value),
            "recordEnd" => RecordEnd,
            "screenshot" => Screenshot(path, value, timeout),
            "spotlight" => Spotlight(Locator(path, value), timeout),
            "type" => Type(path, value, timeout),
            "viewport" => Viewport(path, value),
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

    // While recording, the cursor glides to the element first
    private static Func<SpecRun, Task> Click(Func<IPage, ILocator> locate, float timeout) =>
        run =>
            run.Recording is null
                ? locate(run.Page).ClickAsync(new() { Timeout = timeout })
                : run.Video!.ClickAsync(locate(run.Page), timeout);

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
            await locate(run.Page)
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

    // A locator mapping plus the exact text, e.g. { role: heading, text: Trips }
    private static Func<SpecRun, Task> ExpectText(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "expectText needs a locator and 'text'.");
        }

        var locate = Locator(path, mapping);
        var text = Scalar(path, Child(path, mapping, "text"));

        return run =>
            Assertions.Expect(locate(run.Page)).ToHaveTextAsync(text, new() { Timeout = timeout });
    }

    // Matches the end of the URL, so specs only need the path
    private static Func<SpecRun, Task> ExpectUrl(string url, float timeout) =>
        run =>
            Assertions
                .Expect(run.Page)
                .ToHaveURLAsync(new Regex($"{Regex.Escape(url)}$"), new() { Timeout = timeout });

    private static Func<SpecRun, Task> ExpectVisible(Func<IPage, ILocator> locate, float timeout) =>
        run => Assertions.Expect(locate(run.Page)).ToBeVisibleAsync(new() { Timeout = timeout });

    private static Func<SpecRun, Task> Goto(string path, int line, string url) =>
        async run =>
        {
            var response = await run.Page.GotoAsync(url);
            if (response is not { Ok: true })
            {
                throw new SpecException(path, line, $"{url} returned {response?.Status}.");
            }
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
                : locate(run.Page)
                    .ScreenshotAsync(
                        new()
                        {
                            Animations = ScreenshotAnimations.Disabled,
                            Path = Path.Combine(run.ScreenshotDirectory, $"{elementName}.png"),
                            Timeout = timeout,
                        }
                    );
    }

    // Dims the page except for every element the locator matches; annotations still draw on top
    private static Func<SpecRun, Task> Spotlight(Func<IPage, ILocator> locate, float timeout) =>
        async run =>
        {
            var locator = locate(run.Page);
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
            locate(run.Page)
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
