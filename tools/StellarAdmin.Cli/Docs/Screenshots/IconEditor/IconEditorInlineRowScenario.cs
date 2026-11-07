using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IconEditor;

public sealed class IconEditorInlineRowScenario : Scenario
{
    public override string Description =>
        "The trips grid editing a trip's icon, title and published flag in its row.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Icon, column => column.UseDisplay<Resources.Displays.IconDisplay>());
                    columns.Add(trip => trip.Title);
                    columns.Add(trip => trip.IsPublished);
                });
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            });

            resource.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Icon).UseIconEditor();
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.IsPublished);
                });
                edit.UseInlineRow();
            });
        });
    }
}
