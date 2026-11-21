using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

public sealed class RowDetailsProjectionScenario : Scenario
{
    public override string Description =>
        "Row details that project a booking into its pricing when the row expands.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
                index.AddTable(
                    "table",
                    table =>
                    {
                        table.Columns(RowDetailsBookings.AddColumns);
                        table.EnableRowDetails("_BookingPricing", RowDetailsBookings.Pricing);
                        table.EnablePaging(paging => paging.PageSize = 10);
                    }
                )
            )
        );
    }
}
