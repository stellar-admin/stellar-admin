using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.QuickStart;

public sealed class QuickStartTripsScenario : Scenario
{
    public override string Description =>
        "The trips grid with Code and Title columns, from the quick start.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Code);
                    columns.Add(trip => trip.Title);
                })
            );
        });
    }
}
