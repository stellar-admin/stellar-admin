using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IconEditor;

public sealed class IconEditorPickerScenario : Scenario
{
    public override string Description =>
        "The trip edit page with the trip's icon in an icon editor.";

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
                {
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.Icon).UseIconEditor();
                })
            );
        });
    }
}
