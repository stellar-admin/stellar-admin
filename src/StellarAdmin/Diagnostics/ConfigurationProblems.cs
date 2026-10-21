using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Diagnostics;

// Every configuration problem of the admin panel: those found while the app configured it, those found while
// building each resource's options, and those in the icon registrations
internal sealed class ConfigurationProblems(
    StellarAdminOptions options,
    IEnumerable<ResourceRegistration> resources,
    IOptions<IconOptions> iconOptions,
    ILoggerFactory loggerFactory,
    IServiceProvider services
)
{
    private readonly ConcurrentDictionary<
        (string? Resource, string Message),
        ConfigurationProblem
    > _found = new();
    private IReadOnlyList<ConfigurationProblem>? _problems;

    // The problems found at startup, which never change, followed by those a page found while rendering
    public IReadOnlyList<ConfigurationProblem> GetProblems() =>
        [.. _problems ??= FindProblems(), .. _found.Values];

    public static void Log(ILogger logger, ConfigurationProblem problem) =>
        logger.LogWarning(
            problem.EventId,
            "Configuration problem in {Resource}: {Problem} {Outcome}",
            problem.Resource ?? "the admin panel",
            problem.Message,
            problem.Outcome
        );

    // A problem that only shows when a page renders, such as an editor whose template doesn't match it. It is logged
    // the first time, and listed with the others from then on.
    public void Report(ConfigurationProblem problem, string? resourceSlug)
    {
        var registration = resources.FirstOrDefault(resource =>
            string.Equals(resource.ControllerName, resourceSlug, StringComparison.OrdinalIgnoreCase)
        );
        problem = problem with
        {
            Resource = registration?.ResolveSidebarItem(services).Label ?? problem.Resource,
            ResourceSlug = registration?.ControllerName,
        };
        if (_found.TryAdd((problem.Resource, problem.Message), problem))
        {
            Log(loggerFactory.CreateLogger(ConfigurationProblemsLogger.Category), problem);
        }
    }

    private ConfigurationProblem[] FindProblems()
    {
        var problems = new List<ConfigurationProblem>(options.Problems);
        foreach (var resource in resources)
        {
            var label = resource.ResolveSidebarItem(services).Label;
            problems.AddRange(
                resource
                    .ResolveProblems(services)
                    .Select(problem =>
                        problem with
                        {
                            Resource = label,
                            ResourceSlug = resource.ControllerName,
                        }
                    )
            );
        }

        // StellarAdmin.UI logs the icon problems itself
        problems.AddRange(
            iconOptions.Value.Problems.Select(problem => new ConfigurationProblem(
                ConfigurationProblemKind.SkippedIconMapping,
                problem,
                "The icon keeps its previous mapping."
            )
            {
                Resource = "Icons",
                IsLoggedElsewhere = true,
            })
        );

        return [.. problems];
    }
}
