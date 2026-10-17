using StellarAdmin.UI;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Specifies the size of an <see cref="ImageDisplay" />, which is its height in the grid and
///     its maximum height on the view page.
/// </summary>
public enum ImageDisplaySize
{
    /// <summary>
    ///     24px in the grid and 8rem on the view page.
    /// </summary>
    ExtraSmall,

    /// <summary>
    ///     32px in the grid and 10rem on the view page.
    /// </summary>
    Small,

    /// <summary>
    ///     40px in the grid and 15rem on the view page.
    /// </summary>
    Medium,

    /// <summary>
    ///     64px in the grid and 20rem on the view page.
    /// </summary>
    Large,
}

internal static class ImageDisplaySizeExtensions
{
    extension(ImageDisplaySize size)
    {
        public ImageSize ToImageSize() =>
            size switch
            {
                ImageDisplaySize.ExtraSmall => ImageSize.ExtraSmall,
                ImageDisplaySize.Medium => ImageSize.Medium,
                ImageDisplaySize.Large => ImageSize.Large,
                _ => ImageSize.Small,
            };
    }
}
