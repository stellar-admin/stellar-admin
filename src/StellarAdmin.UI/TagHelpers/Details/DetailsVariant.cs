namespace StellarAdmin.UI;

/// <summary>
///     The visual style of a <c>&lt;sa-details&gt;</c> list.
/// </summary>
public enum DetailsVariant
{
    /// <summary>
    ///     Fields laid out in a grid with space between them.
    /// </summary>
    Default,

    /// <summary>
    ///     Fields stacked in rows separated by rules.
    /// </summary>
    Divided,
}

internal static class DetailsVariantExtensions
{
    extension(DetailsVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                DetailsVariant.Default => "default",
                DetailsVariant.Divided => "divided",
                _ => string.Empty,
            };
    }
}
