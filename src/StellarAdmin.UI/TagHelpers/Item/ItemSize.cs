namespace StellarAdmin.UI;

/// <summary>
///     The size of an item, controlling its padding and spacing.
/// </summary>
public enum ItemSize
{
    /// <summary>The standard item size.</summary>
    Default,

    /// <summary>The most compact item size.</summary>
    ExtraSmall,

    /// <summary>A compact item size.</summary>
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
