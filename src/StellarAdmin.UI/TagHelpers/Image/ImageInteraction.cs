namespace StellarAdmin.UI;

/// <summary>
///     Specifies what pointing at or clicking an image does.
/// </summary>
[GenerateDataAttributeText]
public enum ImageInteraction
{
    /// <summary>
    ///     Clicking opens a larger image in a dialog.
    /// </summary>
    Lightbox,

    /// <summary>
    ///     Pointing shows a larger image beside it.
    /// </summary>
    Preview,

    /// <summary>
    ///     Nothing happens.
    /// </summary>
    None,
}
