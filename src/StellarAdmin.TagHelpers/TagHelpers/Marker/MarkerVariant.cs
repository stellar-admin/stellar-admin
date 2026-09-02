namespace StellarAdmin.TagHelpers;

/// <summary>
///     The layout of a marker.
/// </summary>
public enum MarkerVariant
{
    /// <summary>An inline marker, for status updates, notes and actions.</summary>
    Default,

    /// <summary>A marker with a bottom border separating it from the next row.</summary>
    Border,

    /// <summary>A centered label with a divider line running out to each side.</summary>
    Separator,
}

internal static class MarkerVariantExtensions
{
    extension(MarkerVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                MarkerVariant.Default => "default",
                MarkerVariant.Border => "border",
                MarkerVariant.Separator => "separator",
                _ => string.Empty,
            };
    }
}
