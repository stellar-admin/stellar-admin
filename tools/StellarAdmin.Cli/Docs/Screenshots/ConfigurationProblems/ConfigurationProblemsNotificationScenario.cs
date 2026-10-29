using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ConfigurationProblems;

public sealed class ConfigurationProblemsNotificationScenario : Scenario
{
    public override string Description =>
        "The trips grid with a page size that isn't one of its page sizes, and a default sort on a column that isn't sortable.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Code);
                    columns.Add(trip => trip.Title);
                    columns.Add(trip => trip.DurationDays);
                });
                index.DefaultSortBy(trip => trip.DurationDays);
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 15;
                    paging.PageSizes = [20, 50, 100];
                });
            })
        );
    }
}
