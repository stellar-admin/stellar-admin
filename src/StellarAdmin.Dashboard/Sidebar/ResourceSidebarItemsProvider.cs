using Microsoft.AspNetCore.Http;
using StellarAdmin.Dashboard.Infrastructure.Authorization;
using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.Sidebar;

internal sealed class ResourceSidebarItemsProvider(
    IEnumerable<ResourceRegistration> resources,
    IServiceProvider services
) : ISidebarItemsProvider
{
    public async Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext)
    {
        var items = new List<(string ControllerName, ResourceSidebarItem Item, int Index)>();
        foreach (var (resource, index) in resources.Select((resource, index) => (resource, index)))
        {
            var item = resource.ResolveSidebarItem(services);
            if (
                item.Visible
                && await AuthorizationMetadata.AuthorizeAsync(
                    resource.ResolveAuthorizationMetadata(services),
                    httpContext
                )
            )
            {
                items.Add((resource.ControllerName, item, index));
            }
        }

        return items
            .GroupBy(entry => entry.Item.Group, StringComparer.Ordinal)
            .Select(
                SidebarItem (group) =>
                    new SidebarGroupItem(
                        group.Key,
                        group
                            .OrderBy(entry => entry.Item.Order)
                            .ThenBy(entry => entry.Index)
                            .Select(
                                (entry) =>
                                    new SidebarActionLinkItem(
                                        entry.Item.Label,
                                        entry.ControllerName,
                                        "Index",
                                        "StellarAdmin"
                                    )
                                    {
                                        Order = entry.Item.Order,
                                    }
                            )
                            .Cast<SidebarLinkItemBase>()
                            .ToArray()
                    )
            )
            .ToArray();
    }
}
