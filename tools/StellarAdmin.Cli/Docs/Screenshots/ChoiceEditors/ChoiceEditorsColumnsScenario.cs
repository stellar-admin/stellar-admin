using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

public sealed class ChoiceEditorsColumnsScenario : Scenario
{
    public override string Description =>
        "The trip edit page with the trip's tags as a checkbox group in three columns.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<TripTag>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            });
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(trip => trip.Tags).UseCheckboxGroupEditor(tags => tags.Columns(3)))
            );
        });
    }
}
