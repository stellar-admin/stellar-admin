using SkillsGenerator;
using Spectre.Console;

// Walk from the executable so deterministic builds and unrelated working directories
// resolve the same checkout. Compile-time source paths may be rewritten to /_/ by CI.
static string GetRepoRootFolder()
{
    for (
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        directory is not null;
        directory = directory.Parent
    )
    {
        if (
            File.Exists(Path.Combine(directory.FullName, "StellarAdmin.slnx"))
            && File.Exists(
                Path.Combine(directory.FullName, "util", "SkillsGenerator", "skills.examples.json")
            )
        )
        {
            return directory.FullName;
        }
    }

    throw new DirectoryNotFoundException(
        "Run SkillsGenerator from a product checkout's build output."
    );
}

var repoRoot = GetRepoRootFolder();

// Sources, samples, and consumer references belong to this checkout.
var products = new[]
{
    (
        Package: "StellarAdmin.TagHelpers",
        Skill: "stellar-admin-tag-helpers",
        Title: "StellarAdmin Tag Helpers"
    ),
    (
        Package: "StellarAdmin.Dashboard",
        Skill: "stellar-admin-dashboard",
        Title: "StellarAdmin Dashboard"
    ),
};
var tagHelpersRoots = products
    .Select(product => Path.Combine(repoRoot, "src", product.Package, "TagHelpers"))
    .ToArray();
var pagesRoot = Path.Combine(repoRoot, "docs", "DocsSamples", "Pages");

var checkMode = args.Contains("--check");

var manifestPath = Path.Combine(repoRoot, "util", "SkillsGenerator", "skills.examples.json");
var exampleManifest = Snippets.LoadManifest(manifestPath);

var enums = tagHelpersRoots
    .SelectMany(Extractor.BuildEnumIndex)
    .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
var renderer = new Renderer(enums);
var components = new List<ComponentInfo>();
var outputs = new Dictionary<string, string>(StringComparer.Ordinal);
foreach (var product in products)
{
    var root = Path.Combine(repoRoot, "src", product.Package, "TagHelpers");
    var referencesDir = Path.Combine(repoRoot, "skills", product.Skill, "references");
    var productComponents = Extractor
        .ExtractComponents(
            root,
            enums,
            folder => Snippets.ForComponent(pagesRoot, folder, exampleManifest),
            tagHelpersRoots
        )
        .OrderBy(component => component.FolderName, StringComparer.Ordinal)
        .ToList();
    components.AddRange(productComponents);

    foreach (var component in productComponents)
    {
        var path = Path.Combine(
            referencesDir,
            "components",
            Extractor.ToKebabCase(component.FolderName) + ".md"
        );
        var existing = File.Exists(path) ? File.ReadAllText(path) : null;
        var package = product.Package == "StellarAdmin.TagHelpers" ? null : product.Package;
        outputs[path] = renderer.RenderComponent(component, existing, package);
    }
    outputs[Path.Combine(referencesDir, "components-index.md")] = Renderer.RenderIndex(
        productComponents,
        product.Title
    );
}

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

foreach (var (path, content) in outputs)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content);
}

AnsiConsole.MarkupLine(
    $"[green]Generated[/] {components.Count} component file(s) + {products.Length} product indexes."
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
