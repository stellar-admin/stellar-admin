using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Scopes;

public sealed class ScopesTabsScenario : Scenario
{
    public override string Description =>
        "The bookings grid with All, Pending, Confirmed and Cancelled scopes, All by default.";

    public override string StartPath => "/stellaradmin/bookings?scope=pending";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(booking => booking.Reference);
                    columns.Add(booking => booking.Traveller);
                    columns.Add(booking => booking.Status);
                });
                index.EnableScopes(scopes =>
                {
                    scopes.Add("all", "All");
                    scopes.Add(
                        "pending",
                        "Pending",
                        booking => booking.Status == BookingStatus.Pending
                    );
                    scopes.Add(
                        "confirmed",
                        "Confirmed",
                        booking => booking.Status == BookingStatus.Confirmed
                    );
                    scopes.Add(
                        "cancelled",
                        "Cancelled",
                        booking => booking.Status == BookingStatus.Cancelled
                    );
                    scopes.DefaultScope = "all";
                });
                index.EnablePaging();
            })
        );
        Travellers.Add(admin);
    }
}
