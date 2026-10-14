using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Sidebar;

/// <summary>
///     Supplies items for the admin panel sidebar.
/// </summary>
public interface ISidebarItemsProvider
{
    /// <summary>
    ///     Returns the sidebar items for the current request.
    /// </summary>
    /// <param name="httpContext">The current request.</param>
    Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext);
}
