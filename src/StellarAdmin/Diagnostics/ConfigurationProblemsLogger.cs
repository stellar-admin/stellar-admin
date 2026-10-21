using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.Diagnostics;

// Builds every resource's options when the app starts, so each problem is logged once, in every environment. Strict
// mode stops the app instead.
internal sealed class ConfigurationProblemsLogger(
    ConfigurationProblems problems,
    StellarAdminOptions options,
    ILoggerFactory loggerFactory
) : IHostedService
{
    public const string Category = "StellarAdmin.Configuration";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var found = problems.GetProblems();
        if (options.Diagnostics.ThrowOnProblems && found.Count > 0)
        {
            throw new InvalidOperationException(
                $"The admin panel's configuration has {found.Count} problem{(found.Count == 1 ? "" : "s")}:"
                    + string.Concat(
                        found.Select(problem => $"{Environment.NewLine}- {Describe(problem)}")
                    )
            );
        }

        var logger = loggerFactory.CreateLogger(Category);
        foreach (var problem in found.Where(problem => !problem.IsLoggedElsewhere))
        {
            ConfigurationProblems.Log(logger, problem);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static string Describe(ConfigurationProblem problem) =>
        $"{problem.Resource ?? "Admin panel"}: {problem.Message} {problem.Outcome}"
        + (problem.Source is { } source ? $" Configured at {source}." : "");
}
