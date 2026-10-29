using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Inputs;

public sealed class InputsCodeScenario : Scenario
{
    public override string Description =>
        "A guide's edit page with their 4-digit badge code in a code input.";

    public override string StartPath => "/stellaradmin/guides/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Guide>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(guide => guide.FirstName);
                    fields.Add(guide => guide.LastName);
                    fields.Add(guide => guide.BadgeCode).UseOneTimeCodeEditor(code => code.Length = 4);
                })
            );
        });
    }
}
