using System.Buffers.Binary;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Spectre.Console;
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

    // The names no spec has, and every spec's name for the error that lists them
    public static (IReadOnlyList<string> Unknown, IReadOnlyList<string> All) MatchSpecNames(
        IEnumerable<string> names
    )
    {
        List<string> all = [.. SpecFiles.Find(SourceFolder).Select(SpecFiles.Name)];
        return ([.. names.Except(all)], all);
    }

    // Captures every spec's screenshots, or only the named specs', rewrites the manifest and reports what
    // is still open. A run of every spec deletes the existing screenshots first, unless keepOutput, so
    // renamed or removed ones don't leave files behind; a run of named specs keeps the others.
    public static async Task GenerateAsync(bool keepOutput, IReadOnlyCollection<string> specNames)
    {
        if (!keepOutput && specNames.Count == 0)
        {
            Clean();
            AnsiConsole.MarkupLine("[green]✓[/] Cleaned existing screenshots.");
        }

        var specs = FindSpecs(specNames);
        await AnsiConsole
            .Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn()
            )
            .StartAsync(ctx =>
                RunSpecsAsync(
                    specs,
                    ctx.AddTask("[green]Capturing screenshots[/]", maxValue: specs.Count)
                )
            );
        WriteManifest();
        AnsiConsole.MarkupLineInterpolated($"[green]✓[/] Captured {specs.Count} screenshot specs.");

        Report();
    }

    public static void Clean() => DocsPaths.Delete(OutputFolder, ManifestFile);

    // Lists the docs' placeholders no spec captures and the screenshots no page shows. These are reported,
    // not errors: a page can wait for its screenshot.
    public static void Report()
    {
        var check = ScreenshotCheck.Run(DocsContentFolder, SourceFolder, DocsPaths.RepoRoot);
        foreach (var (placeholder, missing) in check.Open)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]Open screenshot[/] {placeholder.Name} ([grey]{placeholder.Page}:{placeholder.Line}[/]): no spec saves {string.Join(", ", missing)}"
            );
            if (placeholder.Todo is { } todo)
                AnsiConsole.MarkupLineInterpolated($"    [grey]{todo}[/]");
        }

        foreach (var screenshot in check.Unused)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]Unused screenshot[/] {screenshot.Name} ([grey]{screenshot.Spec}[/]): no Screenshot shows it"
            );
        }

        AnsiConsole.MarkupLineInterpolated(
            $"[green]✓[/] Checked screenshots: {check.Open.Count} open, {check.Unused.Count} unused."
        );
    }

    // The spec files to run: every spec, or the ones named (the file name without .spec.yaml)
    private static IReadOnlyList<string> FindSpecs(IReadOnlyCollection<string> names) =>
        [
            .. SpecFiles
                .Find(SourceFolder)
                .Where(spec => names.Count == 0 || names.Contains(SpecFiles.Name(spec))),
        ];

    // Runs each spec against its scenario and saves its screenshots. Each host's log messages are printed
    // after its spec, since a host writing to the console would garble the progress display.
    private static async Task RunSpecsAsync(IReadOnlyList<string> specs, ProgressTask task)
    {
        var catalog = new ScenarioCatalog(typeof(ScreenshotGenerator).Assembly);
        var log = new CollectingLoggerProvider();

        await using var browser = await SpecBrowser.LaunchAsync();
        foreach (var file in specs)
        {
            var name = SpecFiles.Name(file);
            task.Description = $"[green]Capturing[/] {Markup.Escape(name)}";
            await SpecRunner.RunAsync(
                SpecLoader.Load(catalog, file, Path.GetRelativePath(DocsPaths.RepoRoot, file)),
                browser,
                OutputFolder,
                logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(log);
                }
            );

            foreach (var (level, message) in log.Drain())
            {
                var color = level >= LogLevel.Error ? "red" : "yellow";
                AnsiConsole.MarkupLineInterpolated($"[{color}]{level}[/] {name}: {message}");
            }

            task.Increment(1);
        }

        task.Description = "[green]Capturing screenshots[/]";
    }

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
