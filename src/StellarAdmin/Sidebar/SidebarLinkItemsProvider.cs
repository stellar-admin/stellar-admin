using Microsoft.AspNetCore.Http;
using StellarAdmin.Infrastructure.Authorization;

namespace StellarAdmin.Sidebar;

internal sealed class SidebarLinkItemsProvider(StellarAdminOptions options) : ISidebarItemsProvider
{
    public async Task<IReadOnlyList<SidebarItem>> GetItemsAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var items = new List<SidebarItem>();
        foreach (var link in options.SidebarLinks)
        {
            if (
                !await AuthorizationMetadata.AuthorizeAsync(link.AuthorizationMetadata, httpContext)
            )
            {
                continue;
            }

            var item = link.Target.CreateItem(link);

            // Each grouped link gets its own group; SidebarItemsMerger merges groups with the
            // same label and sorts their links.
            items.Add(link.Group is null ? item : new SidebarGroupItem(link.Group, [item]));
        }

        return [.. items];
    }
}
