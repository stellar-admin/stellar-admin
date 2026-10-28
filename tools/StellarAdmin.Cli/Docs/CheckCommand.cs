using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs check
internal sealed class CheckCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        ScreenshotGenerator.Report();
        return 0;
    }
}
