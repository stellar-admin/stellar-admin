using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs check
internal sealed class CheckCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    ) => await ScreenshotGenerator.CheckAsync() ? 0 : 1;
}
