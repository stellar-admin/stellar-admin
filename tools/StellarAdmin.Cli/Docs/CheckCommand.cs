using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// Lists the docs' screenshot placeholders no spec captures, and the screenshots no page shows
internal sealed class CheckCommand : Command
{
    // Open placeholders and unused screenshots are reported, not errors: a page can wait for its screenshot
    public static void Report(ScreenshotCheck check)
    {
        foreach (var (placeholder, missing) in check.Open)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]Open screenshot[/] {placeholder.Name} ([grey]{placeholder.Page}:{placeholder.Line}[/]): no spec saves {string.Join(", ", missing)}"
            );
            if (placeholder.Todo is { } todo)
                AnsiConsole.MarkupLineInterpolated($"    [grey]{todo}[/]");
        }

        foreach (var screenshot in check.Unused)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]Unused screenshot[/] {screenshot.Name} ([grey]{screenshot.Spec}[/]): no Screenshot shows it"
            );
        }

        AnsiConsole.MarkupLineInterpolated(
            $"[green]✓[/] Checked screenshots: {check.Open.Count} open, {check.Unused.Count} unused."
        );
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        Report(ScreenshotGenerator.Check());
        return 0;
    }
}
