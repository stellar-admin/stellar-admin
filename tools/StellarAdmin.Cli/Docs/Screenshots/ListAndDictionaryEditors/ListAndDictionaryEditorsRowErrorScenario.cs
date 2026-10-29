using System.ComponentModel.DataAnnotations;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ListAndDictionaryEditors;

// The seeded trip doesn't validate its highlights, so the create page edits a form model with the docs' validation
public sealed class ListAndDictionaryEditorsRowErrorScenario : Scenario
{
    public override string Description =>
        "A trip create page whose highlights show an error under a highlight that repeats the trip's title.";

    public override string StartPath => "/stellaradmin/trips/create";

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
            resource.AllowCreate<TripHighlightsForm, TripHighlightsFormHandler>(create =>
                create.Fields(fields =>
                {
                    fields.Add(form => form.Title);
                    fields.Add(form => form.Highlights);
                })
            );
        });
    }

    public sealed class TripHighlightsForm : IValidatableObject
    {
        public string Title { get; set; } = "";

        public List<string> Highlights { get; set; } = [];

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            for (var index = 0; index < Highlights.Count; index++)
            {
                if (string.Equals(Highlights[index], Title, StringComparison.OrdinalIgnoreCase))
                    yield return new ValidationResult(
                        "A highlight can't be the trip's name.",
                        [$"{nameof(Highlights)}[{index}]"]
                    );
            }
        }
    }

    // The capture only shows the form after a save that fails validation
    public sealed class TripHighlightsFormHandler : IResourceCreateHandler<TripHighlightsForm>
    {
        public Task<ResourceOperationResult> CreateAsync(
            TripHighlightsForm model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());
    }
}
