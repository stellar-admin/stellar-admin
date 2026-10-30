using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

public sealed class RowDetailsInlineScenario : Scenario
{
    public override string Description =>
        "The bookings grid with row details that render the _BookingSummary partial view with the page.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(RowDetailsBookings.AddColumns);
                index.EnableRowDetails("_BookingSummary");
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
