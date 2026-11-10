using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.RelatedValues;

public sealed class RelatedValuesTagsScenario : Scenario
{
    public override string Description =>
        "The trips grid with each trip's tags as badges.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<TripTag>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title);
                    columns.Add(trip => trip.Tags, column => column.UseDisplay<Resources.Displays.BadgeDisplay>());
                });
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
