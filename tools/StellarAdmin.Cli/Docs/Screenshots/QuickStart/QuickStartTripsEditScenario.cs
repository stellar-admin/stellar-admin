using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.QuickStart;

public sealed class QuickStartTripsEditScenario : Scenario
{
    public override string Description =>
        "The trips resource with create and edit forms, from the quick start.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

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
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(trip => trip.Code);
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.DurationDays);
                })
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Code);
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.DurationDays);
                })
            );
        });
    }
}
