namespace StellarAdmin.UI;

/// <summary>
///     Specifies the kind of media in an item.
/// </summary>
[GenerateDataAttributeText]
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
