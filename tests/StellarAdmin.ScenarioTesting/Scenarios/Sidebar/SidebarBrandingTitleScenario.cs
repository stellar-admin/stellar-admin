using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.ScenarioTesting.Scenarios.Sidebar;

public sealed class SidebarBrandingTitleScenario : Scenario
{
    public override string Description =>
        "A branding title, subtitle and link, on the sidebar header, app header and browser title.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding =>
        {
            branding.Title = "Voyager Travel";
            branding.Subtitle = "Admin Console";
            branding.Href = "~/home";
        });
    }
}
