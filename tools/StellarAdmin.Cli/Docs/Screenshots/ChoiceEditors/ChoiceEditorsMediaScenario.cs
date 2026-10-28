using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

public sealed class ChoiceEditorsMediaScenario : Scenario
{
    public override string Description =>
        "A trip create page with the transport as a toggle group of options with icons.";

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
            resource.AllowCreate(create =>
                create.Fields(fields =>
                    fields
                        .Add(trip => trip.Transport)
                        .UseToggleGroupEditor(toggle =>
                            toggle.UseItems([
                                new ChoiceItem("Plane", "Plane")
                                {
                                    Media = new ItemMedia.Icon("plane"),
                                },
                                new ChoiceItem("Train", "Train")
                                {
                                    Media = new ItemMedia.Icon("train-front"),
                                },
                                new ChoiceItem("Ship", "Ship")
                                {
                                    Media = new ItemMedia.Icon("ship"),
                                },
                            ])
                        )
                )
            );
        });
    }
}
