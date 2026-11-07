using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

public sealed class ChoiceEditorsItemsScenario : Scenario
{
    public override string Description =>
        "A guide create page with the home airport as a radio group of a fixed list of airports.";

    public override string StartPath => "/stellaradmin/guides/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Guide>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate(create =>
                create.Fields(fields =>
                    fields
                        .Add(guide => guide.HomeAirport)
                        .UseRadioGroupEditor(radio =>
                            radio.UseItems([
                                new ChoiceItem("LIS", "Lisbon"),
                                new ChoiceItem("KIX", "Kyoto"),
                                new ChoiceItem("CPT", "Cape Town"),
                            ])
                        )
                )
            );
        });
    }
}
