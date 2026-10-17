namespace StellarAdmin.UI;

/// <summary>
///     Specifies what pointing at or clicking an image does.
/// </summary>
public enum ImageInteraction
{
    /// <summary>
    ///     Clicking opens a larger image in a dialog.
    /// </summary>
    Lightbox,

    /// <summary>
    ///     Pointing shows a larger image beside it.
    /// </summary>
    Preview,

    /// <summary>
    ///     Nothing happens.
    /// </summary>
    None,
}

internal static class ImageInteractionExtensions
{
    extension(ImageInteraction imageInteraction)
    {
        public string GetDataAttributeText() =>
            imageInteraction switch
            {
                ImageInteraction.Lightbox => "lightbox",
                ImageInteraction.Preview => "preview",
                ImageInteraction.None => "none",
                _ => string.Empty,
            };
    }
}
