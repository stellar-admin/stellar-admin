namespace StellarAdmin.UI;

/// <summary>
///     How an image upload control arranges the image and its actions.
/// </summary>
public enum ImageUploadLayout
{
    /// <summary>A large drop target, and once filled the image in its own shape with the actions over it.</summary>
    Dropzone,

    /// <summary>A 64px thumbnail beside the actions.</summary>
    Compact,

    /// <summary>A file row with the file name and the actions.</summary>
    Attachment,

    /// <summary>A thumbnail that picks a file when clicked, with a remove button on its corner, for an editing table row.</summary>
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
