using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Sidebar;

/// <summary>
///     Provides items for the admin panel sidebar.
/// </summary>
/// <remarks>
///     Register a provider as a scoped or singleton <see cref="ISidebarItemsProvider" /> service.
///     The sidebar and the command palette merge the items of every registered provider, so groups
///     with the same label become one group.
/// </remarks>
public interface ISidebarItemsProvider
{
    /// <summary>
    ///     Returns the sidebar items for the current request.
    /// </summary>
    /// <param name="httpContext">The context of the current request.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that returns the sidebar items.</returns>
    Task<IReadOnlyList<SidebarItem>> GetItemsAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken
    );
}
