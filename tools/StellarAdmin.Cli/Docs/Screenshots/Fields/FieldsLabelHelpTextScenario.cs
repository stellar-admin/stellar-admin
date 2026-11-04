using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Fields;

public sealed class FieldsLabelHelpTextScenario : Scenario
{
    public override string Description =>
        "A departure's edit page with the Start date label and help text from the [Display] attribute.";

    public override string StartPath => "/stellaradmin/departures/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Departure>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    // The seed's [Display] has no Description, so the field sets the docs example's help text
                    fields.Add(
                        departure => departure.StartDate,
                        field => field.Description = "The first day of the trip."
                    )
                )
            );
        });
    }
}
