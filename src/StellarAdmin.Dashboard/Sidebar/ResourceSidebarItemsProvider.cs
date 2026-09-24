namespace StellarAdmin.Dashboard.Sidebar;

internal sealed class ResourceSidebarItemsProvider(
    IEnumerable<ResourceSidebarRegistration> resources,
    IServiceProvider services
) : ISidebarItemsProvider
{
    public SidebarItem[] GetItems()
    {
        var items = resources
            .Select(
                (resource, index) =>
                    new
                    {
                        resource.ControllerName,
                        Item = resource.Resolve(services),
                        Index = index,
                    }
            )
            .Where(entry => entry.Item.Visible);

        return items
            .GroupBy(entry => entry.Item.Group, StringComparer.Ordinal)
            .Select(group => new SidebarGroupItem(
                group.Key,
                group
                    .OrderBy(entry => entry.Item.Order)
                    .ThenBy(entry => entry.Index)
                    .Select(entry => new SidebarActionLinkItem(
                        entry.Item.Label,
                        entry.ControllerName,
                        "Index",
                        "StellarAdmin"
                    ))
                    .ToArray()
            ))
            .ToArray();
    }
}
