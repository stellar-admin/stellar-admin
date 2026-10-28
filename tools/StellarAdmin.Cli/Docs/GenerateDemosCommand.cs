using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs generate demos
internal sealed class GenerateDemosCommand : AsyncCommand<GenerateSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        GenerateSettings settings,
        CancellationToken cancellationToken
    )
    {
        await DemoGenerator.GenerateAsync(settings.KeepOutput);
        return 0;
    }
}
