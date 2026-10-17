namespace StellarAdmin.UI;

/// <summary>
///     Specifies how an image upload arranges the image and its actions.
/// </summary>
public enum ImageUploadLayout
{
    /// <summary>
    ///     A large drop target that, once filled, shows the image in its own shape with the actions
    ///     over it.
    /// </summary>
    Dropzone,

    /// <summary>
    ///     A 64px thumbnail beside the actions.
    /// </summary>
    Compact,

    /// <summary>
    ///     A file row with the file name and the actions.
    /// </summary>
    Attachment,

    /// <summary>
    ///     A thumbnail that picks a file when clicked, with a remove button on its corner.
    /// </summary>
    /// <remarks>
    ///     Use it in a table row that is being edited.
    /// </remarks>
    Cell,
}

internal static class ImageUploadLayoutExtensions
{
    extension(ImageUploadLayout imageUploadLayout)
    {
        public string GetDataAttributeText() =>
            imageUploadLayout switch
            {
                ImageUploadLayout.Dropzone => "dropzone",
                ImageUploadLayout.Compact => "compact",
                ImageUploadLayout.Attachment => "attachment",
                ImageUploadLayout.Cell => "cell",
                _ => string.Empty,
            };
    }
}
