namespace StellarAdmin.UI;

/// <summary>
///     Specifies the shape of the box an image is shown in.
/// </summary>
public enum ImageAspectRatio
{
    /// <summary>
    ///     A 1:1 box.
    /// </summary>
    Square,

    /// <summary>
    ///     A 4:3 box.
    /// </summary>
    Landscape,

    /// <summary>
    ///     A 16:9 box.
    /// </summary>
    Widescreen,

    /// <summary>
    ///     A 3:4 box.
    /// </summary>
    Portrait,

    /// <summary>
    ///     The image's own shape.
    /// </summary>
    /// <remarks>
    ///     A thumbnail's width stays between half and three times its height.
    /// </remarks>
    Original,
}

internal static class ImageAspectRatioExtensions
{
    extension(ImageAspectRatio imageAspectRatio)
    {
        public string GetDataAttributeText() =>
            imageAspectRatio switch
            {
                ImageAspectRatio.Square => "square",
                ImageAspectRatio.Landscape => "landscape",
                ImageAspectRatio.Widescreen => "widescreen",
                ImageAspectRatio.Portrait => "portrait",
                ImageAspectRatio.Original => "original",
                _ => string.Empty,
            };
    }
}
