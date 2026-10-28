using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// docs generate screenshots
internal sealed class GenerateScreenshotsCommand : AsyncCommand<GenerateScreenshotsCommand.Settings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        return await ScreenshotGenerator.GenerateAsync(settings.KeepOutput, settings.Specs) ? 0 : 1;
    }

    public sealed class Settings : GenerateSettings
    {
        [CommandOption("--spec <NAME>")]
        [Description(
            "Capture only this spec, by its file name without .spec.yaml. Repeat for more. Keeps the other screenshots."
        )]
        public string[] Specs { get; init; } = [];

        public override ValidationResult Validate()
        {
            var (unknown, all) = ScreenshotGenerator.MatchSpecNames(Specs);
            return unknown.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Error(
                    $"No screenshot spec named {string.Join(", ", unknown)}. The specs are: {string.Join(", ", all)}."
                );
        }
    }
}
