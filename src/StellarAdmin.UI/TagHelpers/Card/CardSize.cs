namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a card, which sets its padding and spacing.
/// </summary>
public enum CardSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size with less padding and spacing.
    /// </summary>
    Small,
}

internal static class CardSizeExtensions
{
    extension(CardSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                CardSize.Default => "default",
                CardSize.Small => "sm",
                _ => string.Empty,
            };
    }
}
