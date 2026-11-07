using System.ComponentModel.DataAnnotations;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

// A form model edits the same non-nullable difficulty three times, once in each toggle group appearance
public sealed class ChoiceEditorsToggleAppearanceScenario : Scenario
{
    public override string Description =>
        "A trip create page with the difficulty as a toggle group in each appearance, with Moderate selected.";

    public override string StartPath => "/stellaradmin/trips/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate<TripDifficultyForm, TripDifficultyFormHandler>(create =>
                create.Fields(fields =>
                {
                    fields
                        .Add(form => form.ChipsDifficulty)
                        .UseToggleGroupEditor(toggle => toggle.Appearance = ToggleGroupAppearance.Chips);
                    fields
                        .Add(form => form.JoinedDifficulty)
                        .UseToggleGroupEditor(toggle => toggle.Appearance = ToggleGroupAppearance.Joined);
                    fields
                        .Add(form => form.ButtonsDifficulty)
                        .UseToggleGroupEditor(toggle => toggle.Appearance = ToggleGroupAppearance.Buttons);
                })
            );
        });
    }

    public sealed class TripDifficultyForm
    {
        [Display(Name = "Difficulty")]
        public TripDifficulty ChipsDifficulty { get; set; } = TripDifficulty.Moderate;

        [Display(Name = "Difficulty")]
        public TripDifficulty JoinedDifficulty { get; set; } = TripDifficulty.Moderate;

        [Display(Name = "Difficulty")]
        public TripDifficulty ButtonsDifficulty { get; set; } = TripDifficulty.Moderate;
    }

    // The capture only shows the form
    public sealed class TripDifficultyFormHandler : IResourceCreateHandler<TripDifficultyForm>
    {
        public Task<ResourceOperationResult> CreateAsync(TripDifficultyForm model, CancellationToken cancellationToken) =>
            Task.FromResult(ResourceOperationResult.Success());
    }
}
