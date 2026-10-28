using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs generate: the demos, then the screenshots
internal sealed class GenerateCommand : AsyncCommand<GenerateSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        GenerateSettings settings,
        CancellationToken cancellationToken
    )
    {
        await DemoGenerator.GenerateAsync(settings.KeepOutput);
        await ScreenshotGenerator.GenerateAsync(settings.KeepOutput, []);
        return 0;
    }
}
