using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.ScenarioTesting.Scenarios.Sidebar;

// One scenario per layout, e.g. sidebar-branding-wordmark-layout-start
[ScenarioArguments(WordmarkLayout.Start)]
[ScenarioArguments(WordmarkLayout.Fill)]
public sealed class SidebarBrandingWordmarkLayoutScenario(WordmarkLayout layout) : Scenario
{
    public override string Description => $"A wordmark with the {layout} layout.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding =>
            branding.Logo = BrandingLogo.Wordmark("~/img/wordmark.svg", layout)
        );
    }
}
