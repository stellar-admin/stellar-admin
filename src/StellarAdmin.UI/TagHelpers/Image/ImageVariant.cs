namespace StellarAdmin.UI;

/// <summary>
///     Specifies where an image is shown, which sets its sizes and its empty and error states.
/// </summary>
public enum ImageVariant
{
    /// <summary>
    ///     A small tile with a fixed height, such as in a table cell.
    /// </summary>
    Thumbnail,

    /// <summary>
    ///     A larger image in the page's flow, such as on a details page.
    /// </summary>
    /// <remarks>
    ///     The image is never wider than its container.
    /// </remarks>
    Figure,
}

internal static class ImageVariantExtensions
{
    extension(ImageVariant imageVariant)
    {
        public string GetDataAttributeText() =>
            imageVariant switch
            {
                ImageVariant.Thumbnail => "thumbnail",
                ImageVariant.Figure => "figure",
                _ => string.Empty,
            };
    }
}
