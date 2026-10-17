namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents an item in the sidebar.
/// </summary>
/// <param name="Label">The label of the item.</param>
public abstract record SidebarItem(string Label);

/// <summary>
///     Represents a labeled group of links in the sidebar.
/// </summary>
/// <param name="Label">The label of the group.</param>
/// <param name="Items">The links in the group.</param>
/// <remarks>
///     Groups with the same label are merged into one.
/// </remarks>
public record SidebarGroupItem(string Label, SidebarLinkItemBase[] Items) : SidebarItem(Label);

/// <summary>
///     Represents a link in the sidebar.
/// </summary>
/// <param name="Label">The label of the link.</param>
public abstract record SidebarLinkItemBase(string Label) : SidebarItem(Label)
{
    /// <summary>
    ///     Gets the name of the icon shown before the link's label, or <see langword="null" /> for
    ///     none.
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    ///     Gets a value that indicates whether the link opens in a new browser tab.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the link opens in a new tab; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    public bool OpenInNewTab { get; init; }

    /// <summary>
    ///     Gets the position of the link among the links around it.
    /// </summary>
    /// <value>
    ///     The position, where lower values come first. The default is 0.
    /// </value>
    public int Order { get; init; }
}

/// <summary>
///     Represents a sidebar link to a URL.
/// </summary>
/// <param name="Label">The label of the link.</param>
/// <param name="Href">The absolute URL or app-relative path that the link goes to.</param>
public record SidebarLinkItem(string Label, string Href) : SidebarLinkItemBase(Label);

/// <summary>
///     Represents a sidebar link to an MVC controller action.
/// </summary>
/// <param name="Label">The label of the link.</param>
/// <param name="Controller">The name of the controller.</param>
/// <param name="Action">The name of the action.</param>
/// <param name="Area">
///     The name of the area, an empty string for an action outside any area, or
///     <see langword="null" /> for the current area.
/// </param>
public record SidebarActionLinkItem(string Label, string Controller, string Action, string? Area)
    : SidebarLinkItemBase(Label);

/// <summary>
///     Represents a sidebar link to a Razor Page.
/// </summary>
/// <param name="Label">The label of the link.</param>
/// <param name="Page">The name of the page.</param>
/// <param name="Area">
///     The name of the area, an empty string for a page outside any area, or
///     <see langword="null" /> for the current area.
/// </param>
public record SidebarPageLinkItem(string Label, string Page, string? Area)
    : SidebarLinkItemBase(Label);
