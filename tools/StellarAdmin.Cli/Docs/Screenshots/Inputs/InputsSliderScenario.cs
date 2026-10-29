using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Inputs;

public sealed class InputsSliderScenario : Scenario
{
    public override string Description =>
        "A trip's edit page with its group size on a slider from 2 to 16 people, with a mark at every second value.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Title);
                    fields
                        .Add(trip => trip.MaxGroupSize)
                        .UseSliderEditor(slider =>
                        {
                            slider.Min = 2;
                            slider.Max = 16;
                            slider.MarkInterval = 2;
                            slider.ValueFormat = "{0} people";
                        });
                })
            );
        });
    }
}
