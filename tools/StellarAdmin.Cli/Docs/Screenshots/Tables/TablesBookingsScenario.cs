using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Tables;

public sealed class TablesBookingsScenario : Scenario
{
    public override string Description =>
        "The bookings index with a Bookings table of three columns, sorted by Booked at, and paging.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
                index.AddTable(
                    "bookings",
                    table =>
                    {
                        table.Columns(columns =>
                        {
                            columns.Add(booking => booking.Reference);
                            columns.Add(booking => booking.Traveller);
                            columns.Add(booking => booking.BookedAt, column => column.Sortable());
                        });
                        table.DefaultSortBy(booking => booking.BookedAt);
                        table.EnablePaging();
                    }
                )
            )
        );
        Travellers.Add(admin);
    }
}
