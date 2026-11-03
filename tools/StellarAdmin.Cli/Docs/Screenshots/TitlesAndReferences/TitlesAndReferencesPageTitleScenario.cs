using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.TitlesAndReferences;

public sealed class TitlesAndReferencesPageTitleScenario : Scenario
{
    public override string Description =>
        "A guide's edit page, titled with the guide's first and last name.";

    public override string StartPath => "/stellaradmin/guides/edit/15";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Guide>(resource =>
        {
            resource.UseTitle(guide => guide.FirstName + " " + guide.LastName);
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns =>
                {
                    columns.Add(guide => guide.FirstName);
                    columns.Add(guide => guide.LastName);
                });
            });
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(guide => guide.FirstName);
                    fields.Add(guide => guide.LastName);
                    fields.Add(guide => guide.Email);
                })
            );
        });
    }
}
