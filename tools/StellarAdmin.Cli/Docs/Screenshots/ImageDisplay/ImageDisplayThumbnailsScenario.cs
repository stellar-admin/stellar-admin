using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ImageDisplay;

public sealed class ImageDisplayThumbnailsScenario : Scenario
{
    public override string Description =>
        "The destinations grid with each destination's hero image as a thumbnail.";

    public override string StartPath => "/stellaradmin/destinations";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Destination>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(destination => destination.Name);
                    columns.Add(
                        destination => destination.HeroImageUrl,
                        column => column.UseDisplay<Resources.Displays.ImageDisplay>()
                    );
                });
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
