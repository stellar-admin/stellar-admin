using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

public sealed class ChoiceEditorsCardsScenario : Scenario
{
    public override string Description =>
        "A trip edit page with the tags as a checkbox group of cards.";

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
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields
                        .Add(trip => trip.Tags)
                        .UseCheckboxGroupEditor(tags =>
                            tags.Appearance = CheckboxGroupAppearance.Cards
                        )
                )
            );
        });
        admin.AddEfCoreResource<TripTag>(resource =>
            resource.Index(index => index.Columns(columns => columns.Add(tag => tag.Name)))
        );
    }
}
