using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ImageDisplay;

// image-display-fallbacks-empty and -broken: Tokyo's hero image display field with no image, and
// with an image that fails to load
[ScenarioArguments("empty")]
[ScenarioArguments("broken")]
public sealed class ImageDisplayFallbacksScenario(string fallback) : Scenario
{
    public override string Description =>
        $"Tokyo's edit page with its hero image as a display field, showing the {fallback} image.";

    public override string StartPath => "/stellaradmin/destinations/edit/" + Tokyo;

    // The seeded Tokyo destination
    public const int Tokyo = 2;

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Destination>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(destination => destination.Name);
                    fields
                        .AddDisplayField(destination => destination.HeroImageUrl)
                        .UseDisplay<Resources.Displays.ImageDisplay>(image =>
                            // Every seeded destination has an image, so the URL fakes each fallback
                            image.ResolveUrl(value =>
                                fallback == "empty" ? null : "/images/missing-" + value
                            )
                        );
                })
            );
        });
    }
}
