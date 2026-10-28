using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// Generates the website's UI component demos, its admin panel screenshots or both. Each deletes its own
// existing output first, so renamed or removed demos and screenshots don't leave files behind.
internal sealed class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        if (settings.Output is null or DocsOutput.Demos)
            await GenerateDemosAsync(settings.KeepOutput);

        if (settings.Output is null or DocsOutput.Screenshots)
        {
            await GenerateScreenshotsAsync(settings);
            CheckCommand.Report(ScreenshotGenerator.Check());
        }

        return 0;
    }

    private static async Task GenerateDemosAsync(bool keepOutput)
    {
        AnsiConsole.Write(new Rule("[bold]StellarAdmin.UI docs demos[/]").LeftJustified());
        AnsiConsole.MarkupLineInterpolated(
            $"Hosting DocsSamples from: [grey]{DemoGenerator.DocsSamplesContentRoot}[/]"
        );

        var generator = new DemoGenerator();
        using var factory = new DocsSamplesApplicationFactory(DemoGenerator.DocsSamplesContentRoot);
        using var client = factory.CreateClient();

        await AnsiConsole
            .Status()
            .StartAsync(
                "Cleaning existing demos...",
                async ctx =>
                {
                    if (!keepOutput)
                    {
                        DemoGenerator.Clean();
                        AnsiConsole.MarkupLine("[green]✓[/] Cleaned existing demos.");
                    }

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
                    maxValue: DemoGenerator.DemoPartials.Length
                );

                foreach (var partial in DemoGenerator.DemoPartials)
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
            $"[green]✓[/] Generated {DemoGenerator.DemoPartials.Length} demo partials."
        );
    }

    // A run of named specs keeps the other specs' screenshots
    private static async Task GenerateScreenshotsAsync(Settings settings)
    {
        if (!settings.KeepOutput && settings.Specs.Length == 0)
        {
            ScreenshotGenerator.Clean();
            AnsiConsole.MarkupLine("[green]✓[/] Cleaned existing screenshots.");
        }

        var specs = ScreenshotGenerator.FindSpecs(settings.Specs);
        await AnsiConsole
            .Status()
            .StartAsync("Capturing screenshots...", _ => ScreenshotGenerator.GenerateAsync(specs));
        AnsiConsole.MarkupLineInterpolated($"[green]✓[/] Captured {specs.Count} screenshot specs.");
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[output]")]
        [Description("demos or screenshots. Generates both when left out.")]
        public DocsOutput? Output { get; init; }

        [CommandOption("--keep-output")]
        [Description("Keep the existing output instead of deleting it first.")]
        public bool KeepOutput { get; init; }

        [CommandOption("--spec <NAME>")]
        [Description(
            "Capture only this screenshot spec, by its file name without .spec.yaml. Repeat for more. Keeps the other screenshots."
        )]
        public string[] Specs { get; init; } = [];

        public override ValidationResult Validate()
        {
            if (Specs.Length == 0)
                return ValidationResult.Success();

            if (Output is not DocsOutput.Screenshots)
                return ValidationResult.Error("--spec applies only to docs generate screenshots.");

            var (unknown, all) = ScreenshotGenerator.MatchSpecNames(Specs);
            return unknown.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Error(
                    $"No screenshot spec named {string.Join(", ", unknown)}. The specs are: {string.Join(", ", all)}."
                );
        }
    }
}
