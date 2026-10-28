using StellarAdmin;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Cli.Docs.Screenshots.Sidebar;

public sealed class SidebarLinksScenario : Scenario
{
    public override string Description =>
        "Only links: the host's Departures page and Invoices action in the Sales group, and a help site at the top level.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddSidebarLink(
            "Departures this week",
            SidebarLinkTarget.Page("/Admin/Departures"),
            link =>
            {
                link.Group = "Sales";
                link.Icon = "calendar";
            }
        );
        admin.AddSidebarLink(
            "Invoices",
            SidebarLinkTarget.Action("Index", "Invoices"),
            link => link.Group = "Sales"
        );
        admin.AddSidebarLink(
            "Help center",
            SidebarLinkTarget.Url("https://help.voyager.travel"),
            link => link.OpenInNewTab = true
        );
    }
}
