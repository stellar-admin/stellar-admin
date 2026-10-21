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
    IServiceProvider services
)
{
    private IReadOnlyList<ConfigurationProblem>? _problems;

    // Resource options are built once and cached, so the list never changes after the first call
    public IReadOnlyList<ConfigurationProblem> GetProblems() => _problems ??= FindProblems();

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
                ConfigurationProblemEffect.Ignored,
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
