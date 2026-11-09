using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.UI;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ImageDisplay;

public sealed class ImageDisplaySizeShapeScenario : Scenario
{
    public override string Description =>
        "Tokyo's view page with an extra-small, landscape hero image that previews on hover.";

    public override string StartPath => "/stellaradmin/destinations/view/2";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Destination>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(destination => destination.Name);
                    fields.Add(
                        destination => destination.HeroImageUrl,
                        field =>
                            field.UseDisplay<Resources.Displays.ImageDisplay>(image =>
                            {
                                image.Size = ImageDisplaySize.ExtraSmall;
                                image.AspectRatio = ImageAspectRatio.Landscape;
                                image.Interaction = ImageDisplayInteraction.Preview;
                            })
                    );
                })
            );
        });
    }
}
