namespace StellarAdmin.Cli.Skills;

// Generates the UI skill's component references from the tag helpers' XML docs and the curated DocsSamples
// examples in skills.examples.json, keeping each reference's hand-written structure region
internal static class SkillsGenerator
{
    private static readonly string RepoRoot = GetRepoRoot();

    private static readonly string ManifestFile = Path.Combine(
        RepoRoot,
        "tools",
        "StellarAdmin.Cli",
        "Skills",
        "skills.examples.json"
    );

    private static readonly string PagesFolder = Path.Combine(
        RepoRoot,
        "docs",
        "DocsSamples",
        "Pages"
    );

    private static readonly (string Package, string Skill, string Title)[] Products =
    [
        ("StellarAdmin.UI", "stellar-admin-ui", "StellarAdmin.UI"),
    ];

    // Rewrites the references. Returns whether every step succeeded.
    public static Task<bool> GenerateAsync()
    {
        References? references = null;
        return new StepRunner()
            .Add("Read tag helpers", () => Run(() => references = Render()))
            .Add("Write references", step => Run(() => Write(references!, step)))
            .RunAsync();
    }

    // Checks the references match what generate would write, without changing them
    public static Task<bool> CheckAsync()
    {
        References? references = null;
        return new StepRunner()
            .Add("Read tag helpers", () => Run(() => references = Render()))
            .Add("Check references", step => Run(() => Check(references!, step)))
            .RunAsync();
    }

    private static Task Run(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    private static References Render()
    {
        var tagHelpersFolders = Products
            .Select(product => Path.Combine(RepoRoot, "src", product.Package, "TagHelpers"))
            .ToArray();
        var examples = Snippets.LoadManifest(ManifestFile);
        var enums = tagHelpersFolders
            .SelectMany(Extractor.BuildEnumIndex)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        var renderer = new Renderer(enums);

        var components = new List<ComponentInfo>();
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (package, skill, title) in Products)
        {
            var referencesFolder = Path.Combine(RepoRoot, "skills", skill, "references");
            var productComponents = Extractor
                .ExtractComponents(
                    Path.Combine(RepoRoot, "src", package, "TagHelpers"),
                    enums,
                    folder => Snippets.ForComponent(PagesFolder, folder, examples),
                    tagHelpersFolders
                )
                .OrderBy(component => component.FolderName, StringComparer.Ordinal)
                .ToList();
            components.AddRange(productComponents);

            foreach (var component in productComponents)
            {
                var path = Path.Combine(
                    referencesFolder,
                    "components",
                    Extractor.ToKebabCase(component.FolderName) + ".md"
                );
                var existing = File.Exists(path) ? File.ReadAllText(path) : null;
                files[path] = renderer.RenderComponent(component, existing);
            }

            files[Path.Combine(referencesFolder, "components-index.md")] = Renderer.RenderIndex(
                productComponents,
                title
            );
        }

        return new References(components, files);
    }

    private static void Write(References references, StepRunner.Step step)
    {
        foreach (var (path, content) in references.Files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        var withoutExample = references
            .Components.Where(component => component.Examples.Count == 0)
            .Select(component => component.FolderName)
            .Order(StringComparer.Ordinal)
            .ToList();
        if (withoutExample.Count > 0)
            step.Warn($"No example for {string.Join(", ", withoutExample)}");

        step.Log(
            $"{references.Components.Count} component references and {Products.Length} index written"
        );
    }

    private static void Check(References references, StepRunner.Step step)
    {
        var stale = references
            .Files.Where(file =>
                !string.Equals(
                    File.Exists(file.Key) ? File.ReadAllText(file.Key) : null,
                    file.Value,
                    StringComparison.Ordinal
                )
            )
            .Select(file => Path.GetRelativePath(RepoRoot, file.Key))
            .Order(StringComparer.Ordinal)
            .ToList();

        foreach (var file in stale)
            step.Warn($"Out of date: {file}");

        if (stale.Count > 0)
            throw new InvalidOperationException(
                $"{stale.Count} of {references.Files.Count} references are out of date. Run skills generate to update them."
            );

        step.Log($"{references.Files.Count} references are up to date");
    }

    // Walks up from the build output, since CI's deterministic builds can rewrite compile-time source paths
    // to /_/, which rules out [CallerFilePath]
    private static string GetRepoRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (File.Exists(Path.Combine(directory.FullName, "StellarAdmin.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Run the CLI from a product checkout's build output.");
    }

    private sealed record References(
        IReadOnlyList<ComponentInfo> Components,
        IReadOnlyDictionary<string, string> Files
    );
}
