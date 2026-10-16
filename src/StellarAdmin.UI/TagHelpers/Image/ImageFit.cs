namespace StellarAdmin.UI;

/// <summary>
///     How an image fills a box whose shape differs from its own.
/// </summary>
public enum ImageFit
{
    /// <summary>Fills the box, cropping the image's edges.</summary>
    Cover,

    /// <summary>Shows the whole image inside the box, on a quiet backdrop.</summary>
    Contain,
}

internal static class ImageFitExtensions
{
    extension(ImageFit imageFit)
    {
        public string GetDataAttributeText() =>
            imageFit switch
            {
                ImageFit.Cover => "cover",
                ImageFit.Contain => "contain",
                _ => string.Empty,
            };
    }
}
