using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ListAndDictionaryEditors;

public sealed class ListAndDictionaryEditorsDictionaryScenario : Scenario
{
    public override string Description =>
        "The trip edit page with the baggage allowance in a dictionary editor with Cabin and Allowance columns.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            });
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields
                        .Add(trip => trip.BaggageAllowance)
                        .UseDictionaryEditor(dictionary =>
                        {
                            dictionary.Key(key => key.Label = "Cabin");
                            dictionary.Value(value =>
                            {
                                value.Label = "Allowance";
                                value.Max = 50;
                                value.Suffix = "kg";
                            });
                        })
                )
            );
        });
    }
}
