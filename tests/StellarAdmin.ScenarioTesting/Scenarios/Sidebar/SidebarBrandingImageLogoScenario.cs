using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.ScenarioTesting.Scenarios.Sidebar;

public sealed class SidebarBrandingImageLogoScenario : Scenario
{
    public override string Description => "An image logo beside the title in the sidebar header.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Logo = BrandingLogo.Image("~/img/logo.svg"));
    }
}
