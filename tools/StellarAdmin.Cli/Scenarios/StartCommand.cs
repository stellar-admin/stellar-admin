using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Scenarios;

// scenarios start
internal sealed class StartCommand : AsyncCommand<StartCommand.Settings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        await ScenarioRunner.StartAsync(ScenarioRunner.Find(settings.Scenario)!, settings.Urls);
        return 0;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<SCENARIO>")]
        [Description("The scenario to host, as scenarios list names it.")]
        public string Scenario { get; init; } = "";

        [CommandOption("--urls <URLS>")]
        [Description("The address the host listens on, such as http://127.0.0.1:5207.")]
        public string? Urls { get; init; }

        public override ValidationResult Validate() =>
            ScenarioRunner.Find(Scenario) is null
                ? ValidationResult.Error(
                    $"No scenario named {Scenario}. Run scenarios list to see them."
                )
                : ValidationResult.Success();
    }
}
