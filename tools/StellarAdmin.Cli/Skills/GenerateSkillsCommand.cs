using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Skills;

// skills generate
internal sealed class GenerateSkillsCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    ) => await SkillsGenerator.GenerateAsync() ? 0 : 1;
}
