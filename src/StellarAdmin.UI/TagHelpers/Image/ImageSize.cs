namespace StellarAdmin.UI;

/// <summary>
///     The height of an image: a thumbnail's height, or a figure's maximum height.
/// </summary>
public enum ImageSize
{
    /// <summary>24px for a thumbnail, 8rem for a figure.</summary>
    ExtraSmall,

    /// <summary>32px for a thumbnail, 10rem for a figure.</summary>
    Small,

    /// <summary>40px for a thumbnail, 15rem for a figure.</summary>
    Medium,

    /// <summary>64px for a thumbnail, 20rem for a figure.</summary>
    Large,
}

internal static class ImageSizeExtensions
{
    extension(ImageSize imageSize)
    {
        public string GetDataAttributeText() =>
            imageSize switch
            {
                ImageSize.ExtraSmall => "xs",
                ImageSize.Small => "sm",
                ImageSize.Medium => "md",
                ImageSize.Large => "lg",
                _ => string.Empty,
            };
    }
}
