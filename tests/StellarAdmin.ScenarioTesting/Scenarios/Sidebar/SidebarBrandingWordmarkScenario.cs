using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.ScenarioTesting.Scenarios.Sidebar;

public sealed class SidebarBrandingWordmarkScenario : Scenario
{
    public override string Description =>
        "A wordmark with a dark variant replaces the title in the sidebar header.";

    public override string Name => "sidebar-branding-wordmark";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding =>
        {
            branding.Title = "Voyager Travel";
            branding.Logo = BrandingLogo.Wordmark("~/img/wordmark.svg", "~/img/wordmark-dark.svg");
        });
    }
}
