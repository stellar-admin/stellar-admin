namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an item, which sets its padding and spacing.
/// </summary>
public enum ItemSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     The most compact size.
    /// </summary>
    ExtraSmall,

    /// <summary>
    ///     A compact size.
    /// </summary>
    Small,
}

internal static class ItemSizeExtensions
{
    extension(ItemSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                ItemSize.Default => "default",
                ItemSize.ExtraSmall => "xs",
                ItemSize.Small => "sm",
                _ => string.Empty,
            };
        }
    }
}
