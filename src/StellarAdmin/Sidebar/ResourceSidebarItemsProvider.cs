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
        var items = new List<SidebarItem>();
        foreach (var resource in resources)
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
                // Each item gets its own group; SidebarItemsMerger merges groups with the same
                // label and sorts their items.
                items.Add(new SidebarGroupItem(item.Group, [CreateLink(resource, item)]));
            }
        }

        return [.. items];
    }

    public static SidebarActionLinkItem CreateLink(
        ResourceRegistration resource,
        ResourceSidebarItem item
    ) =>
        new(item.Label, resource.ControllerName, "Index", "StellarAdmin")
        {
            Icon = item.Icon,
            Order = item.Order,
            Position = resource.SidebarPosition,
        };
}
