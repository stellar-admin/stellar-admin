using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;
using VoyagerTravel.Models;

namespace DocsSamplesGenerator.Captures.Sidebar;

public sealed class SidebarMissingLinkScenario : Scenario
{
    public override string Description =>
        "A Sales group link to a page that doesn't exist, which the Development environment shows in red.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            })
        );
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Sales";
                item.Icon = "ticket";
            });
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(booking => booking.Reference));
            });
        });
        admin.AddSidebarLink(
            "Departures this week",
            SidebarLinkTarget.Page("/Admin/Departures"),
            link =>
            {
                link.Group = "Sales";
                link.Icon = "calendar";
            }
        );
    }
}
