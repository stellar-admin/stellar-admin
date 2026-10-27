using StellarAdmin.Scenarios.Fixtures;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Scenarios;

public sealed class BrandingScenario : Scenario
{
    public const string Subtitle = "Operations";
    public const string Title = "Voyager Travel";

    public override string Description => "A title, subtitle and icon logo in the sidebar header.";

    public override string Name => "branding";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding =>
        {
            branding.Title = Title;
            branding.Subtitle = Subtitle;
            branding.Logo = BrandingLogo.Icon("compass");
        });
        admin.AddTripResource();
    }
}
