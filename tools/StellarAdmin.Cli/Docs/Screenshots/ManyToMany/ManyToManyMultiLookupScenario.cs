using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ManyToMany;

public sealed class ManyToManyMultiLookupScenario : Scenario
{
    public override string Description =>
        "The trip edit page with the trip's tags in a multi-lookup, below the title in a two-column form.";

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
                edit.Fields(fields =>
                {
                    // Two columns, with the title across both, so the multi-lookup takes only half the form's width
                    fields.Columns(2);
                    fields.Add(trip => trip.Title).ColumnSpan(2);
                    fields.Add(trip => trip.Tags);
                })
            );
        });
    }
}
