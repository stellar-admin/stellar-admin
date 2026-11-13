namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a card, which sets its padding and spacing.
/// </summary>
[GenerateDataAttributeText]
public enum CardSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size with less padding and spacing.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
