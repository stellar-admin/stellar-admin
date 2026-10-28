using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Scenarios;

// scenarios run
internal sealed class RunCommand : AsyncCommand<RunCommand.Settings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    ) => await ScenarioRunner.RunAsync(settings.Paths, settings.Output) == 0 ? 0 : 1;

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<PATHS>")]
        [Description("Spec files, or folders to run every spec in.")]
        public string[] Paths { get; init; } = [];

        [CommandOption("--output <DIR>")]
        [Description(
            "Save the specs' screenshots and clips here. Without it, screenshot and record steps are skipped."
        )]
        public string? Output { get; init; }

        public override ValidationResult Validate()
        {
            var missing = Paths
                .Where(path => !File.Exists(path) && !Directory.Exists(path))
                .ToList();
            return missing.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Error(
                    $"No spec file or folder at {string.Join(", ", missing)}."
                );
        }
    }
}
