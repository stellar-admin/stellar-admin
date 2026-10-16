using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     How an <see cref="ImageEditor" /> is laid out on a form.
/// </summary>
public enum ImageEditorLayout
{
    /// <summary>
    ///     A drop zone, which shows the image in its own shape once there is one, with replace and
    ///     remove over it.
    /// </summary>
    Dropzone,

    /// <summary>
    ///     A thumbnail beside upload and remove buttons.
    /// </summary>
    Compact,

    /// <summary>
    ///     A row with the thumbnail and the file's name, like an attachment.
    /// </summary>
    Attachment,
}

internal static class ImageEditorLayoutExtensions
{
    extension(ImageEditorLayout layout)
    {
        public ImageUploadLayout ToImageUploadLayout() =>
            layout switch
            {
                ImageEditorLayout.Compact => ImageUploadLayout.Compact,
                ImageEditorLayout.Attachment => ImageUploadLayout.Attachment,
                _ => ImageUploadLayout.Dropzone,
            };
    }
}
