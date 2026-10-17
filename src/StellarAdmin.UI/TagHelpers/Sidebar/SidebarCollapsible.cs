namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a sidebar collapses.
/// </summary>
public enum SidebarCollapsible
{
    /// <summary>
    ///     The sidebar slides off the screen when collapsed.
    /// </summary>
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

internal static class SidebarCollapsibleExtensions
{
    extension(SidebarCollapsible collapsible)
    {
        public string GetDataAttributeText() =>
            collapsible switch
            {
                SidebarCollapsible.OffCanvas => "offcanvas",
                SidebarCollapsible.Icon => "icon",
                SidebarCollapsible.None => "none",
                _ => string.Empty,
            };
    }
}
