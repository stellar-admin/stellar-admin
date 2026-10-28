using Microsoft.Extensions.Logging;
using Spectre.Console;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.ScenarioTesting.Runner;
using StellarAdmin.ScenarioTesting.Scenarios;

namespace StellarAdmin.Cli.Scenarios;

// Lists, hosts and runs specs against the test scenarios and the docs screenshot scenarios
internal static class ScenarioRunner
{
    private static readonly ScenarioCatalog Catalog = new(
        typeof(Scenario).Assembly,
        typeof(ScenarioRunner).Assembly
    );

    public static Scenario? Find(string name) => Catalog.Find(name);

    public static void List()
    {
        var table = new Table().AddColumns("Scenario", "Source", "Description");
        foreach (var scenario in Catalog.All)
        {
            table.AddRow(
                Markup.Escape(scenario.Name),
                Source(scenario),
                Markup.Escape(scenario.Description)
            );
        }

        AnsiConsole.Write(table);
    }

    // Hosts the scenario on urls, or Kestrel's default address, until it shuts down, such as on Ctrl+C
    public static Task StartAsync(Scenario scenario, string? urls) =>
        ScenarioHost.RunAsync(
            scenario,
            urls,
            addresses =>
                AnsiConsole.MarkupLineInterpolated(
                    $"Running scenario [green]{scenario.Name}[/] on {string.Join(", ", addresses)}: {scenario.Description}"
                )
        );

    // Runs the spec files, and every spec in the folders, saving their screenshots and clips to
    // outputFolder when it's given. Every spec runs even when one fails. Returns whether they all passed.
    public static Task<bool> RunAsync(IEnumerable<string> paths, string? outputFolder)
    {
        List<string> specs =
        [
            .. paths.SelectMany(path =>
                Directory.Exists(path) ? SpecFiles.Find(path) : [Path.GetFullPath(path)]
            ),
        ];
        var output = outputFolder is null ? null : Path.GetFullPath(outputFolder);

        return new StepRunner()
            .Add("Run specs", step => RunSpecsAsync(specs, output, step))
            .RunAsync();
    }

    private static string Source(Scenario scenario) =>
        scenario.GetType().Assembly == typeof(Scenario).Assembly ? "tests" : "docs";

    // Each host's log messages are shown as the step's warnings, since a host writing to the console would
    // garble the step's line
    private static async Task RunSpecsAsync(
        IReadOnlyList<string> specs,
        string? output,
        StepRunner.Step step
    )
    {
        var log = new CollectingLoggerProvider();
        var failed = 0;

        step.SetTotal(specs.Count);
        await using var browser = await SpecBrowser.LaunchAsync();
        foreach (var file in specs)
        {
            var name = SpecFiles.Name(file);
            step.Detail = name;
            try
            {
                var completed = await SpecRunner.RunAsync(
                    SpecLoader.Load(
                        Catalog,
                        file,
                        Path.GetRelativePath(Environment.CurrentDirectory, file)
                    ),
                    browser,
                    output,
                    logging =>
                    {
                        logging.ClearProviders();
                        logging.AddProvider(log);
                    }
                );
                Report(log, name, step);
                step.Log($"passed {name} ({completed} steps)");
            }
            catch (SpecException exception)
            {
                Report(log, name, step);
                step.Error($"failed {exception.Message}");
                failed++;
            }

            step.Increment();
        }

        if (failed == 0)
            step.Log($"{specs.Count} specs passed");
        else
            step.Error($"{failed} of {specs.Count} specs failed");
    }

    private static void Report(CollectingLoggerProvider log, string name, StepRunner.Step step)
    {
        foreach (var (level, message) in log.Drain())
            step.Warn($"{level} {name}: {message}");
    }
}
