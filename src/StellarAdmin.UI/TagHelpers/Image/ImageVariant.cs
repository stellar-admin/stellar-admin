namespace StellarAdmin.UI;

/// <summary>
///     Where an image is shown, which sets its sizes and empty and error states.
/// </summary>
public enum ImageVariant
{
    /// <summary>A small fixed-height tile, such as in a table cell.</summary>
    Thumbnail,

    /// <summary>A larger image in the page's flow, such as on a details page, no wider than its container.</summary>
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
