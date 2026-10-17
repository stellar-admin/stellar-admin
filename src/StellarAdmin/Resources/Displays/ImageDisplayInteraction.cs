using StellarAdmin.UI;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Specifies what an <see cref="ImageDisplay" /> does when the user clicks or hovers over it.
/// </summary>
public enum ImageDisplayInteraction
{
    /// <summary>
    ///     Opens a larger image in a dialog when the user clicks it.
    /// </summary>
    Lightbox,

    /// <summary>
    ///     Shows a larger image beside it when the user hovers over it.
    /// </summary>
    Preview,

    /// <summary>
    ///     Does nothing.
    /// </summary>
    None,
}

internal static class ImageDisplayInteractionExtensions
{
    extension(ImageDisplayInteraction interaction)
    {
        public ImageInteraction ToImageInteraction() =>
            interaction switch
            {
                ImageDisplayInteraction.Preview => ImageInteraction.Preview,
                ImageDisplayInteraction.None => ImageInteraction.None,
                _ => ImageInteraction.Lightbox,
            };
    }
}
