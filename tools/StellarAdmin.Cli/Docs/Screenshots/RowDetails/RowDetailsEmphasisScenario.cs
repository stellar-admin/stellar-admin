using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.UI;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.RowDetails;

// One scenario per emphasis, e.g. row-details-emphasis-rail
[ScenarioArguments(DataGridRowDetailEmphasis.Band)]
[ScenarioArguments(DataGridRowDetailEmphasis.Rail)]
public sealed class RowDetailsEmphasisScenario(DataGridRowDetailEmphasis emphasis) : Scenario
{
    public override string Description =>
        $"The bookings grid with row details set apart with the {emphasis} emphasis.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns => RowDetailsBookings.AddColumns(columns));
                index.EnableRowDetails("_BookingSummary", details => details.Emphasis = emphasis);
                // Ten rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
