using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.ScenarioTesting.Hosting;

// A named admin panel configuration that tests, screenshots and the scenario host all run, on its own copy
// of the Voyager Travel database
public abstract partial class Scenario
{
    public abstract string Description { get; }

    // The class name in kebab case without "Scenario", then any [ScenarioArguments] values, e.g.
    // SidebarBrandingTitleScenario is sidebar-branding-title
    public virtual string Name =>
        string.Join(
            '-',
            [
                Kebab(Regex.Replace(GetType().Name, "Scenario$", "")),
                .. Arguments.Select(value =>
                    Kebab(Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null")
                ),
            ]
        );

    // The page the host opens and screenshots capture
    public virtual string StartPath => "/stellaradmin";

    // The [ScenarioArguments] values the catalog created this scenario with
    internal object?[] Arguments { get; set; } = [];

    public abstract void ConfigureAdmin(StellarAdminBuilder admin);

    public virtual void ConfigureServices(IServiceCollection services) { }

    // Names the scenario in test explorers and data-driven test names
    public override string ToString() => Name;

    // HTTPHeaders and HttpHeaders are both http-headers
    private static string Kebab(string value) =>
        WordBoundary().Replace(value, "-").Replace(' ', '-').Replace('_', '-').ToLowerInvariant();

    [GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
    private static partial Regex WordBoundary();
}
