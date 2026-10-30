using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

public sealed class RowDetailsSinglePanelScenario : Scenario
{
    public override string Description =>
        "Row details with a single Summary panel, which shows without an accordion or tabs.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(RowDetailsBookings.AddColumns);
                index.EnableRowDetails(panels => panels.Add("Summary", "_BookingSummary"));
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
