namespace StellarAdmin.UI;

/// <summary>
///     Specifies how an image fills a box whose shape differs from its own.
/// </summary>
[GenerateDataAttributeText]
public enum ImageFit
{
    /// <summary>
    ///     Fills the box and crops the image's edges.
    /// </summary>
    Cover,

    /// <summary>
    ///     Shows the whole image inside the box, on a muted backdrop.
    /// </summary>
    Contain,
}
