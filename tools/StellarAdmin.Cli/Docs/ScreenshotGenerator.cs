using System.Buffers.Binary;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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

    // Captures every spec's screenshots, or only the named specs', rewrites the manifest and checks what
    // is still open. A run of every spec deletes the existing screenshots first, unless keepOutput, so
    // renamed or removed ones don't leave files behind; a run of named specs keeps the others. Returns
    // whether every step succeeded.
    public static async Task<bool> GenerateAsync(
        bool keepOutput,
        IReadOnlyCollection<string> specNames
    )
    {
        var runner = new StepRunner();
        if (!keepOutput && specNames.Count == 0)
        {
            runner.Add(
                "Clean existing screenshots",
                () =>
                {
                    Clean();
                    return Task.CompletedTask;
                }
            );
        }

        return await runner
            .Add("Capture screenshots", step => RunSpecsAsync(FindSpecs(specNames), step))
            .Add(
                "Write screenshot manifest",
                () =>
                {
                    WriteManifest();
                    return Task.CompletedTask;
                }
            )
            .Add("Check screenshots", Check)
            .RunAsync();
    }

    public static void Clean() => DocsPaths.Delete(OutputFolder, ManifestFile);

    // Checks the screenshots on their own, as docs check
    public static Task<bool> CheckAsync() =>
        new StepRunner().Add("Check screenshots", Check).RunAsync();

    // Lists the docs' placeholders no spec captures and the screenshots no page shows, under the step.
    // These are warnings, not errors: a page can wait for its screenshot. A name that doesn't start with its
    // docs page is an error.
    private static Task Check(StepRunner.Step step)
    {
        var check = ScreenshotCheck.Run(DocsContentFolder, SourceFolder, DocsPaths.RepoRoot);
        foreach (var (placeholder, missing) in check.Open)
        {
            step.Warn(
                $"Open screenshot {placeholder.Name} ({placeholder.Page}:{placeholder.Line}): no spec saves {string.Join(", ", missing)}"
            );
            if (placeholder.Todo is { } todo)
                step.Log($"  {todo}");
        }

        foreach (var screenshot in check.Unused)
            step.Warn(
                $"Unused screenshot {screenshot.Name} ({screenshot.Spec}): no Screenshot shows it"
            );

        // Names that break the rule fail the check, so the folders stay sorted by page
        var docsScenarios = new ScenarioCatalog(typeof(ScreenshotGenerator).Assembly).All.Where(
            scenario =>
                scenario
                    .GetType()
                    .Namespace?.StartsWith(typeof(ScreenshotGenerator).Namespace + ".Screenshots")
                == true
        );
        var misnamed = ScreenshotNames.Check(
            DocsContentFolder,
            SourceFolder,
            DocsPaths.RepoRoot,
            docsScenarios
        );
        foreach (var problem in misnamed)
            step.Error(problem);

        step.Log(
            $"{check.Open.Count} open, {check.Unused.Count} unused, {misnamed.Count} misnamed"
        );

        return Task.CompletedTask;
    }

    // The spec files to run: every spec, or the ones named (the file name without .spec.yaml)
    private static IReadOnlyList<string> FindSpecs(IReadOnlyCollection<string> names) =>
        [
            .. SpecFiles
                .Find(SourceFolder)
                .Where(spec => names.Count == 0 || names.Contains(SpecFiles.Name(spec))),
        ];

    // Runs each spec against its scenario and saves its screenshots. Each host's log messages are shown as
    // the step's warnings, since a host writing to the console would garble the step's line.
    private static async Task RunSpecsAsync(IReadOnlyList<string> specs, StepRunner.Step step)
    {
        var catalog = new ScenarioCatalog(typeof(ScreenshotGenerator).Assembly);
        var log = new CollectingLoggerProvider();

        step.SetTotal(specs.Count);
        await using var browser = await SpecBrowser.LaunchAsync();
        foreach (var file in specs)
        {
            var name = SpecFiles.Name(file);
            step.Detail = name;
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
                step.Warn($"{level} {name}: {message}");

            step.Increment();
        }

        // The docs show a recording's GIF, not its MP4
        foreach (var clip in Directory.EnumerateFiles(OutputFolder, "*.mp4"))
            File.Delete(clip);
    }

    // Width and height from each PNG's IHDR chunk, which starts every PNG at byte 16, and from each GIF's
    // logical screen size at byte 6. A GIF's entry also says it is one, so the website asks for .gif.
    private static void WriteManifest()
    {
        var images = new SortedDictionary<string, object>(StringComparer.Ordinal);
        if (Directory.Exists(OutputFolder))
        {
            foreach (var file in Directory.EnumerateFiles(OutputFolder))
            {
                var extension = Path.GetExtension(file);
                if (extension is not (".png" or ".gif"))
                    continue;

                var header = new byte[24];
                using (var stream = File.OpenRead(file))
                {
                    stream.ReadExactly(header);
                }

                images[Path.GetFileNameWithoutExtension(file)] =
                    extension == ".gif"
                        ? new
                        {
                            width = (int)BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6)),
                            height = (int)BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(8)),
                            format = "gif",
                        }
                        : new
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
