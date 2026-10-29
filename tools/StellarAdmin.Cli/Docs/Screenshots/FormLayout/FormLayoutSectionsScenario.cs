using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.FormLayout;

public sealed class FormLayoutSectionsScenario : Scenario
{
    public override string Description =>
        "The trip edit page with a Trip section in two columns and a Pricing section in three.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        FormLayoutTrips.AddRelatedResources(admin);
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            });
            resource.AllowEdit(edit => edit.Fields(FormLayoutTrips.AddSections));
        });
    }
}
