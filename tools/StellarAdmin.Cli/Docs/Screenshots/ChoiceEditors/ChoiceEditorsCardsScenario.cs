using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

public sealed class ChoiceEditorsCardsScenario : Scenario
{
    public override string Description =>
        "A trip create page with the difficulty as a radio group of cards in three columns.";

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
                        .Add(trip => trip.Difficulty)
                        .UseRadioGroupEditor(difficulty =>
                        {
                            difficulty.Appearance = RadioGroupAppearance.Cards;
                            difficulty.Columns(3);
                        })
                )
            );
        });
    }
}
