using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.UserMenu;

public sealed class UserMenuOpenScenario : Scenario
{
    public override string Description => "The default user menu, for a signed-in agent.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.ConfigureHomePage(home => home.ShowSetupChecklist = false);
    }
}
