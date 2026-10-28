using System.ComponentModel.DataAnnotations;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

// The seeded trip's difficulty is a number, so the create page edits a form model with the docs' enum instead
public sealed class ChoiceEditorsRadioEnumScenario : Scenario
{
    public override string Description =>
        "A trip create page with the difficulty as a radio group of an enum with descriptions.";

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
            resource.AllowCreate<TripDifficultyForm, TripDifficultyFormHandler>(create =>
                create.Fields(fields => fields.Add(form => form.Difficulty).UseRadioGroupEditor())
            );
        });
    }
}

public enum TripDifficulty
{
    [Display(Name = "Easy", Description = "Gentle walks, a few hours a day.")]
    Easy,

    [Display(Name = "Moderate", Description = "Full days on foot, some climbs.")]
    Moderate,

    [Display(Name = "Challenging", Description = "Long days and steep trails.")]
    Challenging,
}

public sealed class TripDifficultyForm
{
    public TripDifficulty Difficulty { get; set; }
}

// The capture only shows the form, which is never saved
public sealed class TripDifficultyFormHandler : IResourceCreateHandler<TripDifficultyForm>
{
    public Task<ResourceOperationResult> CreateAsync(
        TripDifficultyForm model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
