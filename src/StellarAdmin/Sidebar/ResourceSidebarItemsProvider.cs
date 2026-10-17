using Microsoft.AspNetCore.Http;
using StellarAdmin.Infrastructure.Authorization;
using StellarAdmin.Resources;

namespace StellarAdmin.Sidebar;

internal sealed class ResourceSidebarItemsProvider(
    IEnumerable<ResourceRegistration> resources,
    IServiceProvider services
) : ISidebarItemsProvider
{
    public async Task<IReadOnlyList<SidebarItem>> GetItemsAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
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
                                        Icon = entry.Item.Icon,
                                        Order = entry.Item.Order,
                                    }
                            )
                    )
            )
            .ToArray();
    }
}
