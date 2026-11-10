using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.RelatedValues;

public sealed class RelatedValuesTagsTextScenario : Scenario
{
    public override string Description =>
        "The trips grid with each trip's tags as a line of text.";

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
                    columns.Add(
                        trip => string.Join(", ", trip.Tags.Select(tag => tag.Name)),
                        column => column.Title = "Tags"
                    );
                });
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
