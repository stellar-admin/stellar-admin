using System.ComponentModel.DataAnnotations;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Inputs;

// The seeded trip has no bool? property, so the create page edits a form model with a nullable IsPublished
public sealed class InputsNullableBooleanRadioGroupScenario : Scenario
{
    public override string Description => "A trip create page with a nullable Published field as a radio group.";

    public override string StartPath => "/stellaradmin/trips/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate<TripPublishedForm, TripPublishedFormHandler>(create =>
                create.Fields(fields => fields.Add(form => form.IsPublished).UseRadioGroupEditor())
            );
        });
    }

    public sealed class TripPublishedForm
    {
        [Display(Name = "Published")]
        public bool? IsPublished { get; set; }
    }

    // The capture only shows the empty form
    public sealed class TripPublishedFormHandler : IResourceCreateHandler<TripPublishedForm>
    {
        public Task<ResourceOperationResult> CreateAsync(
            TripPublishedForm model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());
    }
}
