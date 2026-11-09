using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IconDisplay;

public sealed class IconDisplayGridScenario : Scenario
{
    public override string Description =>
        "The trips grid with each trip's icon beside its title, type and duration.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title);
                    columns.Add(
                        trip => trip.Icon,
                        column => column.UseDisplay<Resources.Displays.IconDisplay>()
                    );
                    columns.Add(trip => trip.TripType);
                    columns.Add(trip => trip.DurationDays);
                });
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
