using System.ComponentModel.DataAnnotations;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

// The seeded trip has no enum collections, so the create page edits a form model with one field per kind of
// choice editor, each with a value already chosen
public sealed class ChoiceEditorsKindsScenario : Scenario
{
    public override string Description =>
        "A trip create page with a select, radio group, segmented control, checkbox group and toggle group.";

    public override string StartPath => "/stellaradmin/trips/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate<TripChoicesForm, TripChoicesFormHandler>(create =>
                create.Fields(fields =>
                {
                    fields.Add(form => form.TripType).UseSelectEditor();
                    fields.Add(form => form.Difficulty).UseRadioGroupEditor();
                    fields.Add(form => form.RoomType).UseSegmentedControlEditor();
                    fields.Add(form => form.PaymentMethods).UseCheckboxGroupEditor();
                    fields.Add(form => form.Climates).UseToggleGroupEditor();
                })
            );
        });
    }

    public sealed class TripChoicesForm
    {
        [Display(Name = "Trip type")]
        public TripType TripType { get; set; } = TripType.Cultural;

        public TripDifficulty Difficulty { get; set; } = TripDifficulty.Moderate;

        [Display(Name = "Room type")]
        public RoomType RoomType { get; set; } = RoomType.Twin;

        [Display(Name = "Payment methods")]
        public List<PaymentMethod> PaymentMethods { get; set; } = [PaymentMethod.Card, PaymentMethod.Voucher];

        public List<Climate> Climates { get; set; } = [Climate.Tropical, Climate.Mediterranean];
    }

    // The capture only shows the form
    public sealed class TripChoicesFormHandler : IResourceCreateHandler<TripChoicesForm>
    {
        public Task<ResourceOperationResult> CreateAsync(TripChoicesForm model, CancellationToken cancellationToken) =>
            Task.FromResult(ResourceOperationResult.Success());
    }
}
