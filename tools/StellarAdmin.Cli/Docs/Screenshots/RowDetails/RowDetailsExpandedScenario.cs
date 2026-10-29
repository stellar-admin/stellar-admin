using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.RowDetails;

public sealed class RowDetailsExpandedScenario : Scenario
{
    public override string Description =>
        "The bookings grid with row details that render the host app's _BookingSummary partial view.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns => RowDetailsBookings.AddColumns(columns));
                index.EnableRowDetails("_BookingSummary");
                // Ten rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
