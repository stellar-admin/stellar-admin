using StellarAdmin.Shell.Sidebar;

namespace ShellPlayground.Sidebar;

public class SidebarItemsProvider : ISidebarItemsProvider
{
    public SidebarItem[] GetItems() =>
        [
            new SidebarGroupItem(
                "Group 1",
                [new SidebarLinkItem("Item 1", "/admin"), new SidebarLinkItem("Item 2", "/admin")]
            ),
        ];
}
