using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Skills;

// skills check
internal sealed class CheckSkillsCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    ) => await SkillsGenerator.CheckAsync() ? 0 : 1;
}
