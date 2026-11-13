namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a sidebar.
/// </summary>
[GenerateDataAttributeText]
public enum SidebarVariant
{
    /// <summary>
    ///     A sidebar flush against the edge of the screen.
    /// </summary>
    [DataAttributeText("sidebar")]
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
