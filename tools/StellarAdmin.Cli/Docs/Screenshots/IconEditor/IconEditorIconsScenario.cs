using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IconEditor;

public sealed class IconEditorIconsScenario : Scenario
{
    public override string Description =>
        "The trip edit page with the trip's icon in an icon editor that offers five icons.";

    public override string StartPath => "/stellaradmin/trips/edit/3";

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
                    fields.Columns(3);
                    fields.Add(trip => trip.Title).ColumnSpanFull();
                    fields.Add(trip => trip.Icon).UseIconEditor(icon =>
                    {
                        icon.Icons = ["mountain", "sailboat", "utensils", "landmark", "tent"];
                    });
                })
            );
        });
    }
}
