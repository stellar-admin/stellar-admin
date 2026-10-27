using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.ScenarioTesting.Scenarios.Sidebar;
using StellarAdmin.Sidebar;

namespace StellarAdmin.ScenarioTesting.Scenarios;

public static class ScenarioCatalog
{
    public static IReadOnlyList<Scenario> All { get; } =
    [
        new DefaultScenario(),
        new BrandingScenario(),
        new StaffOnlyScenario(),
        new SidebarBrandingNoneScenario(),
        new SidebarBrandingTitleScenario(),
        new SidebarBrandingIconLogoScenario(),
        new SidebarBrandingImageLogoScenario(),
        new SidebarBrandingWordmarkScenario(),
        new SidebarBrandingWordmarkLayoutScenario(WordmarkLayout.Start),
        new SidebarBrandingWordmarkLayoutScenario(WordmarkLayout.Fill),
    ];

    public static Scenario? Find(string name) =>
        All.FirstOrDefault(scenario =>
            string.Equals(scenario.Name, name, StringComparison.OrdinalIgnoreCase)
        );
}
