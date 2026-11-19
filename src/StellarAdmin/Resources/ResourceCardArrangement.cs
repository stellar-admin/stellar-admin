namespace StellarAdmin.Resources;

/// <summary>
///     Specifies how the image and the content are arranged on the cards of a cards layout.
/// </summary>
public enum ResourceCardArrangement
{
    /// <summary>
    ///     The image above the content.
    /// </summary>
    Vertical,

    /// <summary>
    ///     The image beside the content, or above it when the cards are narrow.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     The title and description over the bottom of the image, with the rest of the content below it.
    /// </summary>
    Overlay,
}
