using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IconDisplay;

// Every seeded trip has a registered icon, so the create page shows a form model whose icon isn't one
public sealed class IconDisplayUnregisteredScenario : Scenario
{
    public override string Description =>
        "A trip create page with an icon display field whose value isn't a registered icon.";

    public override string StartPath => "/stellaradmin/trips/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate<TripIconForm, TripIconFormHandler>(create =>
                create.Fields(fields =>
                    fields
                        .AddDisplayField(form => form.Icon)
                        .UseDisplay<Resources.Displays.IconDisplay>()
                )
            );
        });
    }

    public sealed class TripIconForm
    {
        public string Icon { get; set; } = "river-cruise";
    }

    // The capture only shows the form
    public sealed class TripIconFormHandler : IResourceCreateHandler<TripIconForm>
    {
        public Task<ResourceOperationResult> CreateAsync(TripIconForm model, CancellationToken cancellationToken) =>
            Task.FromResult(ResourceOperationResult.Success());
    }
}
