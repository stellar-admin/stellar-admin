using Microsoft.AspNetCore.Http;
using StellarAdmin.Dashboard.Infrastructure.Authorization;

namespace StellarAdmin.Dashboard.Sidebar;

internal sealed class SidebarLinkItemsProvider(StellarAdminDashboardOptions options)
    : ISidebarItemsProvider
{
    public async Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext)
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

            var item = link.Target.CreateItem(link.Label) with
            {
                OpenInNewTab = link.OpenInNewTab,
                Order = link.Order,
            };

            // Each grouped link gets its own group; SidebarItemsMerger merges groups with the
            // same label and sorts their links.
            items.Add(link.Group is null ? item : new SidebarGroupItem(link.Group, [item]));
        }

        return [.. items];
    }
}
