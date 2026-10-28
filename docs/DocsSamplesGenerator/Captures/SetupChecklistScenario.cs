using StellarAdmin;
using StellarAdmin.ScenarioTesting.Hosting;

namespace DocsSamplesGenerator.Captures;

public sealed class SetupChecklistScenario : Scenario
{
    public override string Description =>
        "No resources, so the home page shows the setup checklist with only the branding done.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
    }
}
