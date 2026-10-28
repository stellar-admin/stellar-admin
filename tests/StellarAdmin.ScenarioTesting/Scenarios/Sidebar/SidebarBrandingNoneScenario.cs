using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.ScenarioTesting.Scenarios.Sidebar;

public sealed class SidebarBrandingNoneScenario : Scenario
{
    public override string Description =>
        "No branding: the sidebar header shows the title from the application name.";

    public override void ConfigureAdmin(StellarAdminBuilder admin) { }
}
