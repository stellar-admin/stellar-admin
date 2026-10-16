namespace StellarAdmin.UI;

/// <summary>
///     The shape of the box an image is shown in.
/// </summary>
public enum ImageAspectRatio
{
    /// <summary>1:1.</summary>
    Square,

    /// <summary>4:3.</summary>
    Landscape,

    /// <summary>16:9.</summary>
    Widescreen,

    /// <summary>3:4.</summary>
    Portrait,

    /// <summary>The image's own shape. A thumbnail's width stays between half and three times its height.</summary>
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
