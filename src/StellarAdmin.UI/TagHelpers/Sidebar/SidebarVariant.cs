namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a sidebar.
/// </summary>
public enum SidebarVariant
{
    /// <summary>
    ///     A sidebar flush against the edge of the screen.
    /// </summary>
    Default,

    /// <summary>
    ///     A sidebar that floats, with a border and rounded corners.
    /// </summary>
    Floating,

    /// <summary>
    ///     A sidebar that frames the main content area as an inset panel.
    /// </summary>
    Inset,
}

internal static class SidebarVariantExtensions
{
    extension(SidebarVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                SidebarVariant.Default => "sidebar",
                SidebarVariant.Floating => "floating",
                SidebarVariant.Inset => "inset",
                _ => string.Empty,
            };
    }
}
