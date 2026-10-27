using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.ScenarioTesting.Scenarios.Sidebar;

public sealed class SidebarBrandingIconLogoScenario : Scenario
{
    public override string Description => "An icon logo beside the title in the sidebar header.";

    public override string Name => "sidebar-branding-icon-logo";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Logo = BrandingLogo.Icon("compass"));
    }
}
