using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.BadgeDisplay;

public sealed class BadgeDisplayRelatedGridScenario : Scenario
{
    public override string Description =>
        "The trips grid with each trip's tags as badges with the tags' titles.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<TripTag>();
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title);
                    columns.Add(
                        trip => trip.Tags,
                        column => column.UseDisplay<Resources.Displays.BadgeDisplay>()
                    );
                });
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
