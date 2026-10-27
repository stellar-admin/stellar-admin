using System.Text.RegularExpressions;
using Microsoft.Playwright;
using YamlDotNet.RepresentationModel;

namespace StellarAdmin.Scenarios.Tests.Specs;

// Reads a .spec.yaml file into steps; each step is a single-key mapping such as "- goto: /stellaradmin"
internal static class SpecLoader
{
    // The path is relative to the test output, which mirrors the source folder
    public static Spec Load(string path)
    {
        var stream = new YamlStream();
        using (var reader = File.OpenText(Path.Combine(AppContext.BaseDirectory, path)))
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
        Func<SpecRun, Task> run = kind switch
        {
            "click" => Click(Locator(path, value)),
            "expectText" => ExpectText(path, value),
            "expectUrl" => ExpectUrl(Scalar(path, value)),
            "expectVisible" => ExpectVisible(Locator(path, value)),
            "goto" => Goto(path, Line(node), Scalar(path, value)),
            "screenshot" => Screenshot(Scalar(path, value)),
            _ => throw new SpecException(path, Line(node), $"Unknown step '{kind}'."),
        };

        return new(kind, Line(node), run);
    }

    private static Func<SpecRun, Task> Click(Func<IPage, ILocator> locate) =>
        run => locate(run.Page).ClickAsync();

    // A locator mapping plus the exact text, e.g. { role: heading, text: Trips }
    private static Func<SpecRun, Task> ExpectText(string path, YamlNode value)
    {
        if (value is not YamlMappingNode mapping)
        {
            throw new SpecException(path, Line(value), "expectText needs a locator and 'text'.");
        }

        var locate = Locator(path, mapping);
        var text = Scalar(path, Child(path, mapping, "text"));

        return run => Assertions.Expect(locate(run.Page)).ToHaveTextAsync(text);
    }

    // Matches the end of the URL, so specs only need the path
    private static Func<SpecRun, Task> ExpectUrl(string url) =>
        run => Assertions.Expect(run.Page).ToHaveURLAsync(new Regex($"{Regex.Escape(url)}$"));

    private static Func<SpecRun, Task> ExpectVisible(Func<IPage, ILocator> locate) =>
        run => Assertions.Expect(locate(run.Page)).ToBeVisibleAsync();

    private static Func<SpecRun, Task> Goto(string path, int line, string url) =>
        async run =>
        {
            var response = await run.Page.GotoAsync(url);
            if (response is not { Ok: true })
            {
                throw new SpecException(path, line, $"{url} returned {response?.Status}.");
            }
        };

    private static Func<SpecRun, Task> Screenshot(string name) =>
        async run =>
        {
            if (run.ScreenshotDirectory is null)
            {
                return;
            }

            await run.Page.ScreenshotAsync(
                new()
                {
                    Animations = ScreenshotAnimations.Disabled,
                    Path = Path.Combine(run.ScreenshotDirectory, $"{name}.png"),
                }
            );
        };
}
