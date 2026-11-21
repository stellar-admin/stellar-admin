using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.PartialViews;

public sealed class PartialViewsByStatusScenario : Scenario
{
    public override string Description =>
        "The bookings index with a Bookings table and the _BookingsByStatus partial view, By status selected.";

    public override string StartPath => "/stellaradmin/bookings?layout=by-status&search=VB-10715";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.AllowView(view =>
                view.Fields(fields => fields.Add(booking => booking.Reference))
            );
            resource.Index(index =>
            {
                index.EnableSearch(term => booking => booking.Reference.Contains(term));
                index.AddTable(
                    "bookings",
                    table =>
                    {
                        table.Columns(columns =>
                        {
                            columns.Add(booking => booking.Reference);
                            columns.Add(booking => booking.BookedAt);
                        });
                        table.EnablePaging();
                    }
                );
                index.AddPartial("by-status", "_BookingsByStatus");
            });
        });
    }
}
