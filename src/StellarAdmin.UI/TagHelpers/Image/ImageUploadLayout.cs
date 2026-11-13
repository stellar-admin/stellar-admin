namespace StellarAdmin.UI;

/// <summary>
///     Specifies how an image upload arranges the image and its actions.
/// </summary>
[GenerateDataAttributeText]
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
