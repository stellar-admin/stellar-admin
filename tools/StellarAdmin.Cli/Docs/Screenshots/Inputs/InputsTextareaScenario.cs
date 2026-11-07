using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Inputs;

public sealed class InputsTextareaScenario : Scenario
{
    public override string Description =>
        "A trip's summary in a four-row textarea with a placeholder.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields
                        .Add(trip => trip.Summary)
                        .UseTextareaEditor(textarea =>
                        {
                            textarea.Rows = 4;
                            textarea.Placeholder = "Two or three sentences for the trip card.";
                        })
                )
            );
        });
    }
}
