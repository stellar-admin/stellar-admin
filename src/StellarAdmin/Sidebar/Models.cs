namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents an item in the sidebar.
/// </summary>
public abstract class SidebarItem
{
    /// <summary>
    ///     Gets the label of the item.
    /// </summary>
    public string Label { get; }

    private protected SidebarItem(string label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Label = label;
    }
}

/// <summary>
///     Represents a labeled group of links in the sidebar.
/// </summary>
/// <remarks>
///     Groups with the same label are merged into one.
/// </remarks>
public sealed class SidebarGroupItem : SidebarItem
{
    /// <summary>
    ///     Gets the links in the group.
    /// </summary>
    public IReadOnlyList<SidebarLinkItemBase> Items { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SidebarGroupItem" /> class.
    /// </summary>
    /// <param name="label">The label of the group.</param>
    /// <param name="items">The links in the group.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="label" /> is empty or white space.
    /// </exception>
    public SidebarGroupItem(string label, IEnumerable<SidebarLinkItemBase> items)
        : base(label)
    {
        ArgumentNullException.ThrowIfNull(items);

        Items = [.. items];
    }
}

/// <summary>
///     Represents a link in the sidebar.
/// </summary>
public abstract class SidebarLinkItemBase : SidebarItem
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

    private protected SidebarLinkItemBase(string label)
        : base(label) { }
}

/// <summary>
///     Represents a sidebar link to a URL.
/// </summary>
public sealed class SidebarLinkItem : SidebarLinkItemBase
{
    /// <summary>
    ///     Gets the absolute URL or app-relative path that the link goes to.
    /// </summary>
    public string Href { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SidebarLinkItem" /> class.
    /// </summary>
    /// <param name="label">The label of the link.</param>
    /// <param name="href">The absolute URL or app-relative path that the link goes to.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="href" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="label" /> or <paramref name="href" /> is empty or white space.
    /// </exception>
    public SidebarLinkItem(string label, string href)
        : base(label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        Href = href;
    }
}

/// <summary>
///     Represents a sidebar link to an MVC controller action.
/// </summary>
public sealed class SidebarActionLinkItem : SidebarLinkItemBase
{
    /// <summary>
    ///     Gets the name of the action.
    /// </summary>
    public string Action { get; }

    /// <summary>
    ///     Gets the name of the area.
    /// </summary>
    /// <value>
    ///     The name of the area, an empty string for an action outside any area, or
    ///     <see langword="null" /> for the current area.
    /// </value>
    public string? Area { get; }

    /// <summary>
    ///     Gets the name of the controller.
    /// </summary>
    public string Controller { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SidebarActionLinkItem" /> class.
    /// </summary>
    /// <param name="label">The label of the link.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="action">The name of the action.</param>
    /// <param name="area">
    ///     The name of the area, an empty string for an action outside any area, or
    ///     <see langword="null" /> for the current area.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" />, <paramref name="controller" /> or <paramref name="action" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="label" />, <paramref name="controller" /> or <paramref name="action" />
    ///     is empty or white space.
    /// </exception>
    public SidebarActionLinkItem(string label, string controller, string action, string? area)
        : base(label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        Controller = controller;
        Action = action;
        Area = area;
    }
}

/// <summary>
///     Represents a sidebar link to a Razor Page.
/// </summary>
public sealed class SidebarPageLinkItem : SidebarLinkItemBase
{
    /// <summary>
    ///     Gets the name of the area.
    /// </summary>
    /// <value>
    ///     The name of the area, an empty string for a page outside any area, or
    ///     <see langword="null" /> for the current area.
    /// </value>
    public string? Area { get; }

    /// <summary>
    ///     Gets the name of the page.
    /// </summary>
    public string Page { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SidebarPageLinkItem" /> class.
    /// </summary>
    /// <param name="label">The label of the link.</param>
    /// <param name="page">The name of the page.</param>
    /// <param name="area">
    ///     The name of the area, an empty string for a page outside any area, or
    ///     <see langword="null" /> for the current area.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="page" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="label" /> or <paramref name="page" /> is empty or white space.
    /// </exception>
    public SidebarPageLinkItem(string label, string page, string? area)
        : base(label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(page);

        Page = page;
        Area = area;
    }
}
