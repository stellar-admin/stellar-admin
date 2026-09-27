using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Dashboard.Sidebar;

/// <summary>
///     Supplies items for the Dashboard sidebar.
/// </summary>
public interface ISidebarItemsProvider
{
    /// <summary>
    ///     Returns the sidebar items for the current request.
    /// </summary>
    /// <param name="httpContext">The current request.</param>
    Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext);
}
