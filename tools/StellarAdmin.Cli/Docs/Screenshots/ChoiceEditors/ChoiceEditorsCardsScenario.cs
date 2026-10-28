using System.ComponentModel.DataAnnotations;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

// The seeded trip's difficulty is a number, so the create page edits a form model with the docs' enum instead
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
            resource.AllowCreate<DifficultyForm, DifficultyFormHandler>(create =>
                create.Fields(fields =>
                    fields
                        .Add(form => form.Difficulty)
                        .UseRadioGroupEditor(difficulty =>
                        {
                            difficulty.Appearance = RadioGroupAppearance.Cards;
                            difficulty.Columns(3);
                        })
                )
            );
        });
    }

    public enum Difficulty
    {
        [Display(Name = "Easy", Description = "Gentle walks, a few hours a day.")]
        Easy,

        [Display(Name = "Moderate", Description = "Full days on foot, some climbs.")]
        Moderate,

        [Display(Name = "Challenging", Description = "Long days and steep trails.")]
        Challenging,
    }

    public sealed class DifficultyForm
    {
        public Difficulty Difficulty { get; set; } = Difficulty.Moderate;
    }

    // The capture only shows the form, which is never saved
    public sealed class DifficultyFormHandler : IResourceCreateHandler<DifficultyForm>
    {
        public Task<ResourceOperationResult> CreateAsync(
            DifficultyForm model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());
    }
}
