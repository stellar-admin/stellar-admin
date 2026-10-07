namespace StellarAdmin.Dashboard.Sidebar;

public abstract record SidebarItem(string Label);

public record SidebarGroupItem(string Label, SidebarLinkItemBase[] Items) : SidebarItem(Label);

public abstract record SidebarLinkItemBase(string Label) : SidebarItem(Label)
{
    /// <summary>
    ///     Whether the link opens in a new browser tab.
    /// </summary>
    public bool OpenInNewTab { get; init; }

    /// <summary>
    ///     The link's order within its group.
    /// </summary>
    public int Order { get; init; }
}

public record SidebarLinkItem(string Label, string Href) : SidebarLinkItemBase(Label);

public record SidebarActionLinkItem(string Label, string Controller, string Action, string? Area)
    : SidebarLinkItemBase(Label);

/// <summary>
///     A sidebar link to a Razor Page.
/// </summary>
/// <param name="Label">The link label.</param>
/// <param name="Page">The page name.</param>
/// <param name="Area">The area name.</param>
public record SidebarPageLinkItem(string Label, string Page, string? Area)
    : SidebarLinkItemBase(Label);
