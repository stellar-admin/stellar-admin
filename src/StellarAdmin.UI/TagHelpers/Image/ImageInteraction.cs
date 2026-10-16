namespace StellarAdmin.UI;

/// <summary>
///     What pointing at or clicking an image does.
/// </summary>
public enum ImageInteraction
{
    /// <summary>Clicking opens the image larger in a dialog.</summary>
    Lightbox,

    /// <summary>Hovering shows the image larger beside it.</summary>
    Preview,

    /// <summary>Nothing.</summary>
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
