using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.FieldEditors;

// Each property is one image editor scenario, and its display name describes the scenario. Values
// are file names in wwwroot/images, or uploads the playground's image store saved.
public sealed class ImageGallery : FieldEditorGalleryRecord, IFieldEditorGalleryRecord<ImageGallery>
{
    [Display(Name = "Layout Dropzone (default)")]
    public string? Photo { get; set; }

    [Display(
        Name = "Layout Dropzone · empty",
        Description = "Shown at the top of the destination page."
    )]
    public string? Cover { get; set; }

    [Display(Name = "Layout Compact")]
    public string? Thumbnail { get; set; }

    [Display(Name = "Layout Attachment")]
    public string? Attachment { get; set; }

    [Display(Name = "MaxFileSize 100 KB · AcceptedTypes PNG and WebP")]
    public string? Limited { get; set; }

    [Display(Name = "ClassNames.Image and ClassNames.Note")]
    public string? Styled { get; set; }

    [Editable(false)]
    [Display(Name = "[Editable(false)]")]
    public string? ReadOnlyPhoto { get; set; }

    [Display(Name = "Disabled()")]
    public string? DisabledPhoto { get; set; }

    public static ImageGallery CreateSample() =>
        new()
        {
            Photo = "kyoto.jpg",
            Thumbnail = "lisbon.jpg",
            Attachment = "chiang-mai.jpg",
            Styled = "cape-town.jpg",
            ReadOnlyPhoto = "kyoto.jpg",
            DisabledPhoto = "kyoto.jpg",
        };
}
