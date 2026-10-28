using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs clean demos
internal sealed class CleanDemosCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        DemoGenerator.Clean();
        AnsiConsole.MarkupLine("[green]✓[/] Deleted the generated demos.");
        return 0;
    }
}
