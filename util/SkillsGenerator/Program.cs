using System.Runtime.CompilerServices;
using SkillsGenerator;
using Spectre.Console;

// Program.cs lives at <repoRoot>/util/SkillsGenerator/, so the repository root is two
// directories up. [CallerFilePath] is resolved at compile time and stays correct regardless of
// the working directory.
static string GetRepoRootFolder([CallerFilePath] string sourceFilePath = "") =>
    Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", ".."));

var repoRoot = GetRepoRootFolder();

// Sources, samples, and consumer references belong to this checkout.
string[] tagHelpersRoots =
[
    Path.Combine(repoRoot, "src", "StellarAdmin.TagHelpers", "TagHelpers"),
    Path.Combine(repoRoot, "src", "StellarAdmin.Dashboard", "TagHelpers"),
];
var pagesRoot = Path.Combine(repoRoot, "docs", "DocsSamples", "Pages");
var referencesDir = Path.Combine(
    repoRoot,
    "plugins",
    "stellar-admin",
    "skills",
    "tag-helpers",
    "references"
);
var componentsDir = Path.Combine(referencesDir, "components");
var indexPath = Path.Combine(referencesDir, "components-index.md");

var checkMode = args.Contains("--check");

var manifestPath = Path.Combine(repoRoot, "util", "SkillsGenerator", "skills.examples.json");
var exampleManifest = Snippets.LoadManifest(manifestPath);

var enums = tagHelpersRoots
    .SelectMany(Extractor.BuildEnumIndex)
    .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
var components = tagHelpersRoots
    .SelectMany(root =>
        Extractor.ExtractComponents(
            root,
            enums,
            folder => Snippets.ForComponent(pagesRoot, folder, exampleManifest),
            tagHelpersRoots
        )
    )
    .OrderBy(component => component.FolderName, StringComparer.Ordinal)
    .ToList();

var renderer = new Renderer(enums);

// Build the full set of (path, content) outputs.
var outputs = new Dictionary<string, string>(StringComparer.Ordinal);
foreach (var component in components)
{
    var path = Path.Combine(componentsDir, Extractor.ToKebabCase(component.FolderName) + ".md");
    var existing = File.Exists(path) ? File.ReadAllText(path) : null;
    var package = Directory.Exists(Path.Combine(tagHelpersRoots[1], component.FolderName))
        ? "StellarAdmin.Dashboard"
        : null;
    outputs[path] = renderer.RenderComponent(component, existing, package);
}
outputs[indexPath] = Renderer.RenderIndex(components);

if (checkMode)
{
    var drift = new List<string>();
    foreach (var (path, content) in outputs)
    {
        var current = File.Exists(path) ? File.ReadAllText(path) : null;
        if (!string.Equals(current, content, StringComparison.Ordinal))
        {
            drift.Add(Path.GetRelativePath(repoRoot, path));
        }
    }

    if (drift.Count == 0)
    {
        AnsiConsole.MarkupLine("[green]No drift.[/] All skills reference files are up to date.");
        return 0;
    }

    AnsiConsole.MarkupLine($"[red]Drift detected in {drift.Count} file(s):[/]");
    foreach (var file in drift.OrderBy(f => f, StringComparer.Ordinal))
    {
        AnsiConsole.MarkupLine($"  [yellow]{file.EscapeMarkup()}[/]");
    }

    AnsiConsole.MarkupLine("Run the SkillsGenerator to regenerate them.");
    return 1;
}

Directory.CreateDirectory(componentsDir);
foreach (var (path, content) in outputs)
{
    File.WriteAllText(path, content);
}

AnsiConsole.MarkupLine(
    $"[green]Generated[/] {components.Count} component file(s) + the component index."
);

var withoutExample = components
    .Where(c => c.Examples.Count == 0)
    .Select(c => c.FolderName)
    .OrderBy(name => name, StringComparer.Ordinal)
    .ToList();

if (withoutExample.Count > 0)
{
    AnsiConsole.MarkupLine(
        $"[yellow]No example[/] for: {string.Join(", ", withoutExample).EscapeMarkup()}"
    );
}

return 0;
