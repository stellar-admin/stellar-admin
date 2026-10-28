using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs clean screenshots
internal sealed class CleanScreenshotsCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        ScreenshotGenerator.Clean();
        AnsiConsole.MarkupLine("[green]✓[/] Deleted the generated screenshots.");
        return 0;
    }
}
