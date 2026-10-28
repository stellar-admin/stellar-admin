using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs generate all: the demos, then the screenshots
internal sealed class GenerateAllCommand : AsyncCommand<GenerateSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        GenerateSettings settings,
        CancellationToken cancellationToken
    )
    {
        if (!await DemoGenerator.GenerateAsync(settings.KeepOutput))
            return 1;

        return await ScreenshotGenerator.GenerateAsync(settings.KeepOutput, []) ? 0 : 1;
    }
}
