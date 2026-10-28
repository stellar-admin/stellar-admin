using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// Deletes the generated demos, screenshots or both from the website checkout
internal sealed class CleanCommand : Command<CleanCommand.Settings>
{
    public override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        if (settings.Output is null or DocsOutput.Demos)
        {
            DemoGenerator.Clean();
            AnsiConsole.MarkupLine("[green]✓[/] Deleted the generated demos.");
        }

        if (settings.Output is null or DocsOutput.Screenshots)
        {
            ScreenshotGenerator.Clean();
            AnsiConsole.MarkupLine("[green]✓[/] Deleted the generated screenshots.");
        }

        return 0;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[output]")]
        [Description("demos or screenshots. Deletes both when left out.")]
        public DocsOutput? Output { get; init; }
    }
}
