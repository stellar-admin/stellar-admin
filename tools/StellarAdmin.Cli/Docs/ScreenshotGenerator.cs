using System.Buffers.Binary;
using System.Text.Json;
using StellarAdmin.ScenarioTesting.Runner;
using StellarAdmin.ScenarioTesting.Scenarios;

namespace StellarAdmin.Cli.Docs;

// Captures the admin panel screenshots for the website's admin docs, from the scenarios and specs in
// Screenshots/, and matches them against the docs pages' <Screenshot> placeholders
internal static class ScreenshotGenerator
{
    // The scenarios and specs the screenshots are captured from, read from source
    private static readonly string SourceFolder = Path.Combine(
        DocsPaths.RepoRoot,
        "tools",
        "StellarAdmin.Cli",
        "Docs",
        "Screenshots"
    );

    private static readonly string OutputFolder = Path.Combine(
        DocsPaths.WebsiteRoot,
        "public",
        "images",
        "admin",
        "generated"
    );

    // Each generated screenshot's size by name, which the website's Screenshot component reads
    private static readonly string ManifestFile = Path.Combine(
        DocsPaths.WebsiteRoot,
        "src",
        "generated",
        "screenshots.json"
    );

    // The docs pages, which place the screenshots with <Screenshot name="..."> tags
    private static readonly string DocsContentFolder = Path.Combine(
        DocsPaths.WebsiteRoot,
        "content",
        "docs"
    );

    // The spec files to run: every spec, or the ones named (the file name without .spec.yaml)
    public static IReadOnlyList<string> FindSpecs(IReadOnlyCollection<string> names) =>
        [
            .. SpecFiles
                .Find(SourceFolder)
                .Where(spec => names.Count == 0 || names.Contains(SpecFiles.Name(spec))),
        ];

    // The names no spec has, and every spec's name for the error that lists them
    public static (IReadOnlyList<string> Unknown, IReadOnlyList<string> All) MatchSpecNames(
        IEnumerable<string> names
    )
    {
        List<string> all = [.. SpecFiles.Find(SourceFolder).Select(SpecFiles.Name)];
        return ([.. names.Except(all)], all);
    }

    // Runs each spec against its scenario, saves its screenshots and rewrites the manifest
    public static async Task GenerateAsync(IReadOnlyList<string> specs)
    {
        var catalog = new ScenarioCatalog(typeof(ScreenshotGenerator).Assembly);

        await using var browser = await SpecBrowser.LaunchAsync();
        foreach (var file in specs)
        {
            await SpecRunner.RunAsync(
                SpecLoader.Load(catalog, file, Path.GetRelativePath(DocsPaths.RepoRoot, file)),
                browser,
                OutputFolder
            );
        }

        WriteManifest();
    }

    public static void Clean() => DocsPaths.Delete(OutputFolder, ManifestFile);

    public static ScreenshotCheck Check() =>
        ScreenshotCheck.Run(DocsContentFolder, SourceFolder, DocsPaths.RepoRoot);

    // Width and height from each PNG's IHDR chunk, which starts every PNG at byte 16
    private static void WriteManifest()
    {
        var images = new SortedDictionary<string, object>(StringComparer.Ordinal);
        if (Directory.Exists(OutputFolder))
        {
            foreach (var file in Directory.EnumerateFiles(OutputFolder, "*.png"))
            {
                var header = new byte[24];
                using (var stream = File.OpenRead(file))
                {
                    stream.ReadExactly(header);
                }

                images[Path.GetFileNameWithoutExtension(file)] = new
                {
                    width = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16)),
                    height = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20)),
                };
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(ManifestFile)!);
        File.WriteAllText(
            ManifestFile,
            JsonSerializer.Serialize(images, new JsonSerializerOptions { WriteIndented = true })
                + "\n"
        );
    }
}
