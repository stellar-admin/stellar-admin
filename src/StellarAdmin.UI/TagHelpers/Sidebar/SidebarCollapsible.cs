namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a sidebar collapses.
/// </summary>
[GenerateDataAttributeText]
public enum SidebarCollapsible
{
    /// <summary>
    ///     The sidebar slides off the screen when collapsed.
    /// </summary>
    [DataAttributeText("offcanvas")]
    OffCanvas,

    /// <summary>
    ///     The sidebar shrinks to a narrow strip of icons when collapsed.
    /// </summary>
    Icon,

    /// <summary>
    ///     The sidebar can't be collapsed.
    /// </summary>
    None,
}
