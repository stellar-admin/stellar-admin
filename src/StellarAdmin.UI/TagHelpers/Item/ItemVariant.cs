namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of an item.
/// </summary>
[GenerateDataAttributeText]
public enum ItemVariant
{
    /// <summary>
    ///     A plain item with no border or background.
    /// </summary>
    Default,

    /// <summary>
    ///     An item with a border.
    /// </summary>
    Outline,

    /// <summary>
    ///     An item with a muted background.
    /// </summary>
    Muted,
}
