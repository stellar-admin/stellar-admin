using Spectre.Console;

namespace StellarAdmin.Cli;

// Runs steps in order as a checklist: one line per step with a spinner, then ✓ or ✗, and a grey detail,
// plus a bar and percentage once the step sets a total. Each step has its own live line, so its messages
// and any error print under it before the next step starts. A failed step stops the run.
internal sealed class StepRunner
{
    private const int BarWidth = 40;

    private static readonly Spinner Spinner = Spinner.Known.Dots;

    private readonly List<(string Title, Func<Step, Task> Run)> steps = [];

    public StepRunner Add(string title, Func<Task> run) => Add(title, _ => run());

    public StepRunner Add(string title, Func<Step, Task> run)
    {
        steps.Add((title, run));
        return this;
    }

    // Returns whether every step succeeded
    public async Task<bool> RunAsync()
    {
        // Padding every title to the longest keeps the bars aligned across the steps' lines
        var width = steps.Max(step => step.Title.Length);
        foreach (var (title, run) in steps)
        {
            if (!await RunStepAsync(title.PadRight(width), run))
                return false;
        }

        return true;
    }

    // A Live line this redraws for the spinner. Spectre's Progress pads each display with a blank line
    // above and below, which leaves gaps between the steps.
    private static async Task<bool> RunStepAsync(string title, Func<Step, Task> run)
    {
        var step = new Step();
        Exception? error = null;

        await AnsiConsole
            .Live(Line(title, step, $"[yellow]{Spinner.Frames[0]}[/]"))
            .StartAsync(async ctx =>
            {
                var work = run(step);
                for (var frame = 0; !work.IsCompleted; frame++)
                {
                    var spinner = Spinner.Frames[frame % Spinner.Frames.Count];
                    ctx.UpdateTarget(Line(title, step, $"[yellow]{spinner}[/]"));
                    await Task.WhenAny(work, Task.Delay(Spinner.Interval));
                }

                try
                {
                    await work;
                }
                catch (Exception exception)
                {
                    error = exception;
                }

                step.Detail = "";
                ctx.UpdateTarget(Line(title, step, error is null ? "[green]✓[/]" : "[red]✗[/]"));
            });

        // The line has stopped updating, so printing is safe
        foreach (var (color, message) in step.Messages)
            AnsiConsole.MarkupLineInterpolated($"    [{color}]{message}[/]");

        if (error is not null)
            AnsiConsole.WriteException(error, ExceptionFormats.ShortenEverything);

        return error is null;
    }

    // The status markup, the title, the bar and percentage when the step has a total, and the detail
    private static Markup Line(string title, Step step, string status)
    {
        var line = $"{status} {Markup.Escape(title)}";
        if (step.Total is { } total)
        {
            var fraction = Math.Clamp(step.Value / total, 0, 1);
            var filled = (int)Math.Round(fraction * BarWidth);
            line +=
                $" [green]{new string('━', filled)}[/][grey23]{new string('━', BarWidth - filled)}[/] {fraction * 100, 3:0}%";
        }

        if (step.Detail.Length > 0)
            line += $" [grey]{Markup.Escape(step.Detail)}[/]";

        return new Markup(line) { Overflow = Overflow.Ellipsis };
    }

    // What a step's work reports to its line
    public sealed class Step
    {
        internal double? Total { get; private set; }

        internal double Value { get; private set; }

        internal List<(string Color, string Message)> Messages { get; } = [];

        // Grey text at the end of the line, such as the item being worked on
        public string Detail { get; set; } = "";

        // Shows a bar and percentage that fill as the step increments towards total; without one the line
        // shows only the spinner
        public void SetTotal(double total) => Total = total;

        public void Increment(double amount = 1) => Value += amount;

        // Printed under the step's line once it finishes
        public void Log(string message) => Messages.Add(("grey", message));

        public void Warn(string message) => Messages.Add(("yellow", message));
    }
}
