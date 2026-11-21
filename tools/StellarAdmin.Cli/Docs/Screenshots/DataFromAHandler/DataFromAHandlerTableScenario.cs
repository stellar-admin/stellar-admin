using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.DataFromAHandler;

public sealed class DataFromAHandlerTableScenario : Scenario
{
    public override string Description =>
        "The bookings index with a Bookings table and a Revenue per destination table loaded by a handler.";

    public override string StartPath => "/stellaradmin/bookings?layout=revenue";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.EnableSearch(term => booking => booking.Reference.Contains(term));
                index.AddTable(
                    "bookings",
                    table =>
                    {
                        table.Columns(columns => columns.Add(booking => booking.Reference));
                        table.EnablePaging();
                    }
                );
                index.AddTable<DestinationRevenue, DestinationRevenueHandler>(
                    "revenue",
                    table =>
                    {
                        table.Title = "Revenue per destination";
                        table.Icon = "chart-column";
                        table.Columns(columns =>
                        {
                            columns.Add(item => item.Destination);
                            columns.Add(item => item.Bookings);
                            columns.Add(
                                item => item.Revenue,
                                column =>
                                    column.UseDisplay<NumberDisplay>(number => number.Format = "C0")
                            );
                        });
                    }
                );
            })
        );
    }
}
