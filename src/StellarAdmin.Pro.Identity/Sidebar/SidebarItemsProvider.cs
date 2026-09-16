using StellarAdmin.Pro.Identity.Infrastructure;
using StellarAdmin.Pro.Identity.Options;
using StellarAdmin.Pro.Sidebar;

namespace StellarAdmin.Pro.Identity.Sidebar;

internal class SidebarItemsProvider(
    StellarAdminIdentityOptions options,
    IEnumerable<IdentityResourceRegistration> resources
) : ISidebarItemsProvider
{
    public SidebarItem[] GetItems()
    {
        var sidebar = options.Sidebar;

        if (!sidebar.Visible)
        {
            return [];
        }

        // Label cascade: configured item label → page title → library default.
        SidebarLinkItemBase[] items =
        [
            .. resources
                .Where(resource => resource.SidebarItem.Visible)
                .OrderBy(resource => resource.Order)
                .Select(resource => new SidebarActionLinkItem(
                    resource.SidebarItem.Label ?? resource.IndexTitle(),
                    resource.ControllerName,
                    "Index",
                    "StellarAdmin"
                )),
        ];

        // The group exists to hold its items, so it is omitted rather than rendered empty.
        if (items.Length == 0)
        {
            return [];
        }

        return [new SidebarGroupItem(sidebar.Title ?? "Identity", items)];
    }
}
