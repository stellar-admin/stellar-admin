namespace StellarAdmin.UI;

/// <summary>
///     Additional CSS classes for the parts of an image upload.
/// </summary>
public class ImageUploadClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the image upload's container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for the image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    ///     Additional CSS classes for the note under the image upload, which shows the accepted
    ///     types, upload progress, errors and staged changes.
    /// </summary>
    public string? Note { get; set; }
}
