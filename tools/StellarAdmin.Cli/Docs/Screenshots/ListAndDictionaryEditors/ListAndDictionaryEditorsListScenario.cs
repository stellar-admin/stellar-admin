using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ListAndDictionaryEditors;

public sealed class ListAndDictionaryEditorsListScenario : Scenario
{
    public override string Description =>
        "The trip edit page with the highlights in a numbered, sortable list editor of at most eight items.";

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
                        .Add(trip => trip.Highlights)
                        .UseListEditor(list =>
                        {
                            list.MaxItems = 8;
                            list.Sortable = true;
                            list.Numbered = true;
                            list.MaxLength = 120;
                            list.AddText = "Add highlight";
                        })
                )
            );
        });
    }
}
