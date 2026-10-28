using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs clean: the demos and the screenshots
internal sealed class CleanCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        DemoGenerator.Clean();
        ScreenshotGenerator.Clean();
        AnsiConsole.MarkupLine("[green]✓[/] Deleted the generated demos and screenshots.");
        return 0;
    }
}
