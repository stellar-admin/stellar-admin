namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a toggle.
/// </summary>
public enum ToggleVariant
{
    /// <summary>
    ///     A toggle with no border.
    /// </summary>
    Default,

    /// <summary>
    ///     A toggle with a border.
    /// </summary>
    Outline,
}

internal static class ToggleVariantExtensions
{
    extension(ToggleVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                ToggleVariant.Default => "default",
                ToggleVariant.Outline => "outline",
                _ => string.Empty,
            };
    }
}
