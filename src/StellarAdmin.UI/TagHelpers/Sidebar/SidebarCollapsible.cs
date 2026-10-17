namespace StellarAdmin.UI;

/// <summary>
///     How a sidebar behaves when it is collapsed.
/// </summary>
public enum SidebarCollapsible
{
    /// <summary>
    ///     The sidebar slides off-canvas when collapsed.
    /// </summary>
    OffCanvas,

    /// <summary>
    ///     The sidebar collapses to a narrow icon rail when collapsed.
    /// </summary>
    Icon,

    /// <summary>
    ///     The sidebar cannot be collapsed.
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
