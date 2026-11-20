using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Layouts;

public sealed class LayoutsTitleIconScenario : Scenario
{
    public override string Description =>
        "The bookings index with a Bookings table and a Finance table with a receipt icon, Finance selected.";

    public override string StartPath => "/stellaradmin/bookings?layout=finance";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.AddTable(
                    "bookings",
                    table =>
                    {
                        table.Columns(columns =>
                        {
                            columns.Add(booking => booking.Reference);
                            columns.Add(booking => booking.Traveller);
                            columns.Add(booking => booking.BookedAt);
                        });
                        table.EnablePaging();
                    }
                );
                index.AddTable(
                    "finance",
                    table =>
                    {
                        table.Title = "Finance";
                        table.Icon = "receipt";
                        table.Columns(columns =>
                        {
                            columns.Add(booking => booking.Reference);
                            columns.Add(booking => booking.Status);
                            columns.Add(booking => booking.TotalAmount);
                        });
                        table.EnablePaging();
                    }
                );
            })
        );
        Travellers.Add(admin);
    }
}
