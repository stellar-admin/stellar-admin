using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Forms;

public sealed class FormsAddAnotherScenario : Scenario
{
    public override string Description =>
        "The trip create page with a Save and add another trip button that keeps the destination for the next trip.";

    public override string StartPath => "/stellaradmin/trips/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        // Registered so the destination field is a lookup
        admin.AddEfCoreResource<Destination>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.AllowCreate(create =>
            {
                create.Fields(fields =>
                {
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.DestinationId);
                    fields.Add(trip => trip.DurationDays);
                });
                create.EnableAddAnother(another =>
                {
                    another.KeepForNext(trip => trip.DestinationId);
                    another.Label = "Save and add another trip";
                });
            });
        });
    }
}
