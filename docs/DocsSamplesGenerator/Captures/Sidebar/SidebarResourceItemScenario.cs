using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace DocsSamplesGenerator.Captures.Sidebar;

public sealed class SidebarResourceItemScenario : Scenario
{
    public override string Description =>
        "Bookings moved into a Sales group as All bookings, first, with a ticket icon, after the default Resources group.";

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
        admin.AddEfCoreResource<Destination>(resource =>
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(destination => destination.Name));
            })
        );
        admin.AddEfCoreResource<Traveller>(resource =>
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(traveller => traveller.Email));
            })
        );
        admin.AddEfCoreResource<Payment>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Sales";
                item.Icon = "credit-card";
            });
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(payment => payment.Amount));
            });
        });
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Sales";
                item.Label = "All bookings";
                item.Icon = "ticket";
                item.Order = 1;
            });
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(booking => booking.Reference));
            });
        });
    }
}
