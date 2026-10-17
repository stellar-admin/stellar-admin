using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Sidebar;

/// <summary>
///     Provides items for the admin panel sidebar.
/// </summary>
/// <remarks>
///     The sidebar merges the items of every registered provider.
/// </remarks>
public interface ISidebarItemsProvider
{
    /// <summary>
    ///     Returns the sidebar items for the current request.
    /// </summary>
    /// <param name="httpContext">The context of the current request.</param>
    /// <returns>A task that returns the sidebar items.</returns>
    Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext);
}
