using StellarAdmin;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Cli.Docs.Screenshots.Branding;

// One scenario per kind of logo, e.g. branding-logo-icon. A wordmark has no subtitle, which it doesn't display
[ScenarioArguments("none")]
[ScenarioArguments("icon")]
[ScenarioArguments("image")]
[ScenarioArguments("wordmark")]
public sealed class BrandingLogoScenario(string logo) : Scenario
{
    public override string Description => $"The Voyager Travel branding with the {logo} logo.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding =>
        {
            branding.Title = "Voyager Travel";
            if (logo != "wordmark")
                branding.Subtitle = "Admin";

            branding.Logo = logo switch
            {
                "icon" => BrandingLogo.Icon("compass"),
                "image" => BrandingLogo.Image("~/img/logo.svg"),
                "wordmark" => BrandingLogo.Wordmark(
                    "~/img/wordmark.svg",
                    "~/img/wordmark-dark.svg"
                ),
                _ => null,
            };
        });
    }
}
