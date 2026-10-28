using StellarAdmin;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace DocsSamplesGenerator.Captures.Branding;

// One scenario per layout, e.g. branding-wordmark-layout-fill
[ScenarioArguments(WordmarkLayout.Center)]
[ScenarioArguments(WordmarkLayout.Start)]
[ScenarioArguments(WordmarkLayout.Fill)]
public sealed class BrandingWordmarkLayoutScenario(WordmarkLayout layout) : Scenario
{
    public override string Description => $"The Voyager Travel wordmark with the {layout} layout.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding =>
        {
            branding.Title = "Voyager Travel";
            branding.Logo = BrandingLogo.Wordmark(
                "~/img/wordmark.svg",
                "~/img/wordmark-dark.svg",
                layout
            );
        });
    }
}
