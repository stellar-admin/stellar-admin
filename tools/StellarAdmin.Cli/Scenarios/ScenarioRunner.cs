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

    // Hosts the scenario until Ctrl+C, on urls or the host's default address
    public static async Task StartAsync(Scenario scenario, string? urls)
    {
        var app = ScenarioHost.Create(scenario, urls is null ? [] : ["--urls", urls]);
        AnsiConsole.MarkupLineInterpolated(
            $"Running scenario [green]{scenario.Name}[/]: {scenario.Description}"
        );
        await app.RunAsync();
    }

    // Runs the spec files, and every spec in the folders, saving their screenshots and clips to
    // outputFolder when it's given. Returns the number that failed.
    public static async Task<int> RunAsync(IEnumerable<string> paths, string? outputFolder)
    {
        List<string> specs =
        [
            .. paths.SelectMany(path =>
                Directory.Exists(path) ? SpecFiles.Find(path) : [Path.GetFullPath(path)]
            ),
        ];
        var output = outputFolder is null ? null : Path.GetFullPath(outputFolder);

        var failed = await AnsiConsole
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
                    output,
                    ctx.AddTask("[green]Running specs[/]", maxValue: specs.Count)
                )
            );

        if (failed == 0)
            AnsiConsole.MarkupLineInterpolated($"[green]✓[/] {specs.Count} specs passed.");
        else
            AnsiConsole.MarkupLineInterpolated(
                $"[red]✗[/] {failed} of {specs.Count} specs failed."
            );

        return failed;
    }

    private static string Source(Scenario scenario) =>
        scenario.GetType().Assembly == typeof(Scenario).Assembly ? "tests" : "docs";

    // Each host's log messages are printed after its spec, since a host writing to the console would
    // garble the progress display
    private static async Task<int> RunSpecsAsync(
        IReadOnlyList<string> specs,
        string? output,
        ProgressTask task
    )
    {
        var log = new CollectingLoggerProvider();
        var failed = 0;

        await using var browser = await SpecBrowser.LaunchAsync();
        foreach (var file in specs)
        {
            var name = SpecFiles.Name(file);
            task.Description = $"[green]Running[/] {Markup.Escape(name)}";
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
                log.Print(name);
                AnsiConsole.MarkupLineInterpolated($"[green]passed[/] {name} ({completed} steps)");
            }
            catch (SpecException exception)
            {
                log.Print(name);
                AnsiConsole.MarkupLineInterpolated($"[red]failed[/] {exception.Message}");
                failed++;
            }

            task.Increment(1);
        }

        task.Description = "[green]Running specs[/]";

        return failed;
    }
}
