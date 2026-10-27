using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using YamlDotNet.RepresentationModel;

namespace StellarAdmin.Scenarios.Specs;

// Reads a .spec.yaml file into steps; each step is a single-key mapping such as "- goto: /stellaradmin"
public static class SpecLoader
{
    // Draws an outline around the element, with an optional numbered badge on its corner and a label
    // below it (above it near the bottom of the viewport); overlays carry data-spec-annotation
    private const string AnnotateScript = """
        (element, { label, badge }) => {
            const color = "#e11d48";
            const box = element.getBoundingClientRect();
            const left = box.left + window.scrollX;
            const top = box.top + window.scrollY;
            const add = (style, text) => {
                const overlay = document.createElement("div");
                overlay.dataset.specAnnotation = "";
                overlay.textContent = text ?? "";
                Object.assign(overlay.style, {
                    position: "absolute",
                    zIndex: "2147483647",
                    pointerEvents: "none",
                    boxSizing: "border-box",
                    font: "600 13px/1.4 system-ui, sans-serif",
                    color: "#fff",
                    ...style,
                });
                document.body.append(overlay);

                return overlay;
            };

            add({
                left: `${left - 4}px`,
                top: `${top - 4}px`,
                width: `${box.width + 8}px`,
                height: `${box.height + 8}px`,
                border: `2px solid ${color}`,
                borderRadius: "6px",
            });

            if (badge) {
                add({
                    left: `${left - 14}px`,
                    top: `${top - 14}px`,
                    width: "22px",
                    height: "22px",
                    borderRadius: "11px",
                    background: color,
                    textAlign: "center",
                    lineHeight: "22px",
                }, badge);
            }

            if (label) {
                const note = add({
                    left: `${left - 4}px`,
                    padding: "3px 8px",
                    borderRadius: "4px",
                    background: color,
                    whiteSpace: "nowrap",
                }, label);
                const below = box.bottom + 8 + note.offsetHeight <= window.innerHeight;
                note.style.top = below
                    ? `${top + box.height + 8}px`
                    : `${top - 8 - note.offsetHeight}px`;
            }
        }
        """;

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

        return new(path, scenario, [.. steps.Select(step => Step(path, step))]);
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

    private static string? OptionalScalar(string path, YamlMappingNode mapping, string key) =>
        mapping.Children.TryGetValue(new YamlScalarNode(key), out var value)
            ? Scalar(path, value)
            : null;

    private static string Scalar(string path, YamlNode node) =>
        node is YamlScalarNode { Value: { } value }
            ? value
            : throw new SpecException(path, Line(node), "Expected a single value.");

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
            "screenshot" => Screenshot(path, value, timeout),
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

    private static Func<SpecRun, Task> Click(Func<IPage, ILocator> locate, float timeout) =>
        run => locate(run.Page).ClickAsync(new() { Timeout = timeout });

    // { locator, label, badge }; the overlays stay until clearAnnotations, so a screenshot after this
    // step includes them
    private static Func<SpecRun, Task> Annotate(string path, YamlNode value, float timeout)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "annotate needs a 'locator'.");
        }

        var locate = Locator(path, Child(path, mapping, "locator"));
        var label = OptionalScalar(path, mapping, "label");
        var badge = OptionalScalar(path, mapping, "badge");

        return run =>
            locate(run.Page)
                .EvaluateAsync(AnnotateScript, new { label, badge }, new() { Timeout = timeout });
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
