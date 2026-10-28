using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Scenarios;

// scenarios list
internal sealed class ListCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        ScenarioRunner.List();
        return 0;
    }
}
