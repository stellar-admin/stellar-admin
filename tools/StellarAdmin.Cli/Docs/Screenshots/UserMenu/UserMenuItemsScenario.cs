using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Cli.Docs.Screenshots.UserMenu;

public sealed class UserMenuItemsScenario : Scenario
{
    public override string Description =>
        "The user menu with a link to the host's Identity account page and a line before Log out.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.ConfigureHomePage(home => home.ShowSetupChecklist = false);
        admin.ConfigureUserMenu(menu =>
        {
            menu.Add(UserMenuItem.Page("Account", "/Account/Manage/Index", "Identity", item =>
                item.Icon = "badge-check"));
            menu.Add(UserMenuItem.Separator());
        });
    }
}
