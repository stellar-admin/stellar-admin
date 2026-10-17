namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a field set's legend.
/// </summary>
public enum FieldLegendVariant
{
    /// <summary>
    ///     Styled as a legend.
    /// </summary>
    Legend,

    /// <summary>
    ///     Styled like a field label.
    /// </summary>
    Label,
}

internal static class FieldLegendVariantExtensions
{
    extension(FieldLegendVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                FieldLegendVariant.Legend => "legend",
                FieldLegendVariant.Label => "label",
                _ => string.Empty,
            };
    }
}
