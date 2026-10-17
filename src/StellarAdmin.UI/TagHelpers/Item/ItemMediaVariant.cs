namespace StellarAdmin.UI;

/// <summary>
///     Specifies the kind of media in an item.
/// </summary>
public enum ItemMediaVariant
{
    /// <summary>
    ///     Media with no special size or style.
    /// </summary>
    Default,

    /// <summary>
    ///     A small icon.
    /// </summary>
    Icon,

    /// <summary>
    ///     An image or avatar that fills the media area.
    /// </summary>
    Image,
}

internal static class ItemMediaVariantExtensions
{
    extension(ItemMediaVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                ItemMediaVariant.Default => "default",
                ItemMediaVariant.Icon => "icon",
                ItemMediaVariant.Image => "image",
                _ => "",
            };
    }
}
