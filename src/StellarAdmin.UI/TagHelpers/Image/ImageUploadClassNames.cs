namespace StellarAdmin.UI;

/// <summary>
///     Holds the CSS classes added to the parts of an image upload.
/// </summary>
public class ImageUploadClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets the CSS classes added to the image upload's container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    ///     Gets or sets the CSS classes added to the note under the image upload.
    /// </summary>
    /// <remarks>
    ///     The note shows the accepted types, upload progress, errors and staged changes.
    /// </remarks>
    public string? Note { get; set; }
}
