using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Inputs;

public sealed class InputsTextScenario : Scenario
{
    public override string Description =>
        "A trip's base price with a euro prefix, and a traveller's phone number with a placeholder.";

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
                        .Add(trip => trip.BasePrice)
                        .UseTextInputEditor(input =>
                        {
                            input.Prefix = "€";
                            input.Min = 0;
                            input.Step = 10;
                        })
                )
            );
        });
        admin.AddEfCoreResource<Traveller>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields
                        .Add(traveller => traveller.Phone)
                        .UseTextInputEditor(input =>
                        {
                            input.Type = TextInputType.Tel;
                            input.Placeholder = "+351 912 345 678";
                        })
                )
            );
        });
    }
}
