using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ImageEditor;

// image-editor-layout-dropzone, -compact and -attachment: Tokyo's hero image in each layout
[ScenarioArguments("dropzone")]
[ScenarioArguments("compact")]
[ScenarioArguments("attachment")]
public sealed class ImageEditorLayoutScenario(string layout) : Scenario
{
    public override string Description =>
        $"Tokyo's edit page with its hero image in the {layout} image editor layout.";

    public override string StartPath => "/stellaradmin/destinations/edit/" + Tokyo;

    // The seeded Tokyo destination, whose hero image is in the host's wwwroot
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
                        .Add(destination => destination.HeroImageUrl)
                        .UseImageEditor(image =>
                        {
                            // Captures don't upload, so the image is never stored
                            image.StoreImage(
                                (file, services, cancellationToken) =>
                                    Task.FromResult(file.FileName)
                            );
                            image.Layout = layout switch
                            {
                                "compact" => ImageEditorLayout.Compact,
                                "attachment" => ImageEditorLayout.Attachment,
                                _ => ImageEditorLayout.Dropzone,
                            };
                        });
                })
            );
        });
    }
}
