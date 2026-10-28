using Spectre.Console;

namespace DocsSamplesGenerator;

public static class Program
{
    // No arguments generates everything. "captures" only captures the admin panel screenshots, and
    // "captures --check" only lists the docs' Capture placeholders no spec saves, and the shots no page shows.
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var generator = new Generator();

            if (args is ["captures", ..])
            {
                if (args is not [_, "--check"])
                {
                    generator.CleanCapturesOutputFolder();
                    await CaptureScenariosAsync(generator);
                }

                return ReportCaptures(generator.CheckCaptures());
            }

            AnsiConsole.Write(new Rule("[bold]StellarAdmin.UI docs generator[/]").LeftJustified());
            AnsiConsole.MarkupLineInterpolated(
                $"Hosting DocsSamples from: [grey]{Generator.DocsSamplesContentRoot}[/]"
            );

            using var factory = new DocsSamplesApplicationFactory(Generator.DocsSamplesContentRoot);
            using var client = factory.CreateClient();

            await AnsiConsole
                .Status()
                .StartAsync(
                    "Cleaning existing output...",
                    async ctx =>
                    {
                        generator.CleanOutputFolders();
                        AnsiConsole.MarkupLine("[green]✓[/] Cleaned existing output.");

                        ctx.Status("Downloading fixed static assets...");
                        await generator.DownloadFixedStaticAssetsAsync(client);
                        AnsiConsole.MarkupLine("[green]✓[/] Downloaded fixed static assets.");

                        ctx.Status("Downloading dynamic static assets...");
                        await generator.DownloadDynamicStaticAssetsAsync(client);
                        AnsiConsole.MarkupLine("[green]✓[/] Downloaded dynamic static assets.");

                        ctx.Status("Downloading preset stylesheets...");
                        await generator.DownloadPresetStylesheetsAsync(client);
                        AnsiConsole.MarkupLine("[green]✓[/] Downloaded preset stylesheets.");

                        ctx.Status("Downloading demo icons...");
                        await generator.DownloadDemoIconsAsync(client);
                        AnsiConsole.MarkupLine("[green]✓[/] Downloaded demo icons.");

                        ctx.Status("Downloading theme variable manifest...");
                        await generator.DownloadThemeVariableManifestAsync(client);
                        AnsiConsole.MarkupLine("[green]✓[/] Downloaded theme variable manifest.");
                    }
                );

            await AnsiConsole
                .Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn()
                )
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask(
                        "[green]Generating demos[/]",
                        maxValue: Generator.DemoPartials.Length
                    );

                    foreach (var partial in Generator.DemoPartials)
                    {
                        await generator.GenerateDemoPartialSourceFileAsync(partial.Name);
                        await generator.RenderDemoPartialOutputAsync(
                            client,
                            partial.Name,
                            partial.Layout,
                            partial.OutputFileName
                        );

                        task.Increment(1);
                    }
                });

            AnsiConsole.MarkupLineInterpolated(
                $"[green]✓[/] Generated {Generator.DemoPartials.Length} demo partials."
            );

            await CaptureScenariosAsync(generator);
            return ReportCaptures(generator.CheckCaptures());
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Generation failed:[/]");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static async Task CaptureScenariosAsync(Generator generator)
    {
        var captures = await AnsiConsole
            .Status()
            .StartAsync("Capturing scenarios...", _ => generator.CaptureScenariosAsync());
        AnsiConsole.MarkupLineInterpolated($"[green]✓[/] Captured {captures} scenario specs.");
    }

    // Open placeholders and unused shots are reported, not errors: a page can wait for its screenshot
    private static int ReportCaptures(CaptureCheck check)
    {
        foreach (var (placeholder, missing) in check.Open)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]Open capture[/] {placeholder.Name} ([grey]{placeholder.Page}:{placeholder.Line}[/]): no spec saves {string.Join(", ", missing)}"
            );
            if (placeholder.Todo is { } todo)
                AnsiConsole.MarkupLineInterpolated($"    [grey]{todo}[/]");
        }

        foreach (var shot in check.Unused)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]Unused shot[/] {shot.Name} ([grey]{shot.Spec}[/]): no Capture shows it"
            );
        }

        AnsiConsole.MarkupLineInterpolated(
            $"[green]✓[/] Checked captures: {check.Open.Count} open, {check.Unused.Count} unused."
        );
        return 0;
    }
}
