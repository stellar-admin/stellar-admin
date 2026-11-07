using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ImageEditor;

public sealed class ImageEditorInlineRowScenario : Scenario
{
    public override string Description =>
        "The destinations grid editing a destination's hero image, name and featured flag in its row.";

    public override string StartPath => "/stellaradmin/destinations";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Destination>(resource =>
        {
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(
                        destination => destination.HeroImageUrl,
                        column =>
                        {
                            column.Title = "Image";
                            column.UseDisplay<Resources.Displays.ImageDisplay>();
                        }
                    );
                    columns.Add(destination => destination.Name);
                    columns.Add(destination => destination.IsFeatured);
                });
                // Five rows, the destinations with hero images in the host's wwwroot
                index.EnablePaging(paging => paging.PageSize = 5);
            });

            resource.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields
                        .Add(destination => destination.HeroImageUrl)
                        .UseImageEditor(image =>
                            // Captures don't upload, so the image is never stored
                            image.StoreImage((file, services, cancellationToken) => Task.FromResult(file.FileName))
                        );
                    fields.Add(destination => destination.Name);
                    fields.Add(destination => destination.IsFeatured);
                });
                edit.UseInlineRow();
            });
        });
    }
}
