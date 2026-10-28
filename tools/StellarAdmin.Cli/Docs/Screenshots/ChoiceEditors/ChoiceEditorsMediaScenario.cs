using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ChoiceEditors;

// The seeded trip has no transport, so the create page edits a form model with one instead
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
            resource.AllowCreate<TripTransportForm, TripTransportFormHandler>(create =>
                create.Fields(fields =>
                    fields
                        .Add(form => form.Transport)
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

public sealed class TripTransportForm
{
    public string Transport { get; set; } = "";
}

// The capture only shows the form, which is never saved
public sealed class TripTransportFormHandler : IResourceCreateHandler<TripTransportForm>
{
    public Task<ResourceOperationResult> CreateAsync(
        TripTransportForm model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
