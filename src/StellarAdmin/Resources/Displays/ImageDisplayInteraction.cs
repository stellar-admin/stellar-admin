using StellarAdmin.UI;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     What an <see cref="ImageDisplay" /> does when the user clicks or hovers over it.
/// </summary>
public enum ImageDisplayInteraction
{
    /// <summary>
    ///     A click opens a larger image in a dialog.
    /// </summary>
    Lightbox,

    /// <summary>
    ///     Hovering shows a larger image beside it.
    /// </summary>
    Preview,

    /// <summary>
    ///     Nothing.
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
