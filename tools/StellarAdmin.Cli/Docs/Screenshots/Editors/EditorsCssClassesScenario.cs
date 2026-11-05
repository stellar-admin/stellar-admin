using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Editors;

public sealed class EditorsCssClassesScenario : Scenario
{
    public override string Description =>
        "A trip's edit page with the trip-summary class on the Summary textarea, styled as a restricted field.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddStylesheet("~/css/trip-summary.css");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Title);
                    fields
                        .Add(trip => trip.Summary)
                        .UseTextareaEditor(textarea => textarea.ClassNames.Control = "trip-summary");
                })
            );
        });
    }
}
