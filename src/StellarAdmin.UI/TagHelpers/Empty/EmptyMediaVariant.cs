namespace StellarAdmin.UI;

/// <summary>
///     The visual style of a <c>&lt;sa-empty-media&gt;</c> region.
/// </summary>
public enum EmptyMediaVariant
{
    /// <summary>
    ///     Shows the media as is, for illustrations or images.
    /// </summary>
    Default,

    /// <summary>
    ///     Shows a single icon in a muted container.
    /// </summary>
    Icon,
}

internal static class EmptyMediaVariantExtensions
{
    extension(EmptyMediaVariant variant)
    {
        public string GetDataAttributeText()
        {
            return variant switch
            {
                EmptyMediaVariant.Default => "default",
                EmptyMediaVariant.Icon => "icon",
                _ => string.Empty,
            };
        }
    }
}
