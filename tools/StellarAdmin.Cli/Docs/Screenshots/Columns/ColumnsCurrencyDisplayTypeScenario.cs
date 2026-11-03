using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Columns;

public sealed class ColumnsCurrencyDisplayTypeScenario : Scenario
{
    public override string Description =>
        "The bookings grid with reference, traveller, status and total columns, the total rendered as a currency.";

    public override string StartPath => "/stellaradmin/bookings";

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
                    columns.Add(
                        booking => booking.TotalAmount,
                        column => column.UseDisplay<NumberDisplay>(number => number.Format = "C")
                    );
                });
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging =>
                {
                    paging.PageSizes = [5, 10, 25];
                    paging.PageSize = 5;
                });
            })
        );
        Travellers.Add(admin);
    }
}
