namespace StellarAdmin.Shell.Sidebar;

public abstract record SidebarItem(string Label);

public record SidebarGroupItem(string Label, SidebarLinkItemBase[] Items) : SidebarItem(Label);

public abstract record SidebarLinkItemBase(string Label) : SidebarItem(Label);

public record SidebarLinkItem(string Label, string Href) : SidebarLinkItemBase(Label);

public record SidebarActionLinkItem(string Label, string Controller, string Action, string? Area)
    : SidebarLinkItemBase(Label);
