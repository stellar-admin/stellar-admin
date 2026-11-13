namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an item, which sets its padding and spacing.
/// </summary>
[GenerateDataAttributeText]
public enum ItemSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     The most compact size.
    /// </summary>
    [DataAttributeText("xs")]
    ExtraSmall,

    /// <summary>
    ///     A compact size.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
