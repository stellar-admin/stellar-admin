using StellarAdmin;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Cli.Docs.Screenshots.HostPages;

public sealed class HostPagesDeparturesScenario : Scenario
{
    public override string Description =>
        "The host's Departures this week page inside the admin panel, linked from the Sales group in the sidebar.";

    public override string StartPath => "/Admin/Departures";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddSidebarLink(
            "Departures this week",
            SidebarLinkTarget.Page("/Admin/Departures"),
            link => link.Group = "Sales"
        );
    }
}
