namespace StellarAdmin.Sidebar;

/// <summary>
///     The destination of a sidebar link.
/// </summary>
public abstract class SidebarLinkTarget
{
    private protected SidebarLinkTarget() { }

    /// <summary>
    ///     Returns a target for an MVC controller action outside any area.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <param name="controller">The controller name.</param>
    /// <exception cref="ArgumentException"></exception>
    public static SidebarLinkTarget Action(string action, string controller)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);

        return new ActionTarget(action, controller, null);
    }

    /// <summary>
    ///     Returns a target for an MVC controller action in an area.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <param name="controller">The controller name.</param>
    /// <param name="area">The area name.</param>
    /// <exception cref="ArgumentException"></exception>
    public static SidebarLinkTarget Action(string action, string controller, string area)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(area);

        return new ActionTarget(action, controller, area);
    }

    /// <summary>
    ///     Returns a target for a Razor Page outside any area.
    /// </summary>
    /// <param name="page">The page name, e.g. <c>/Reports/Index</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static SidebarLinkTarget Page(string page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(page);

        return new PageTarget(page, null);
    }

    /// <summary>
    ///     Returns a target for a Razor Page in an area.
    /// </summary>
    /// <param name="page">The page name, e.g. <c>/Reports/Index</c>.</param>
    /// <param name="area">The area name.</param>
    /// <exception cref="ArgumentException"></exception>
    public static SidebarLinkTarget Page(string page, string area)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(page);
        ArgumentException.ThrowIfNullOrWhiteSpace(area);

        return new PageTarget(page, area);
    }

    /// <summary>
    ///     Returns a target for a URL.
    /// </summary>
    /// <param name="url">An absolute URL, or an app-relative path such as <c>~/reports</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static SidebarLinkTarget Url(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new UrlTarget(url);
    }

    internal abstract SidebarLinkItemBase CreateItem(string label);

    // Admin panel pages render in the StellarAdmin area, which link generation keeps as an ambient
    // value unless the link names another area. An empty area links outside any area.
    private sealed class ActionTarget(string action, string controller, string? area)
        : SidebarLinkTarget
    {
        internal override SidebarLinkItemBase CreateItem(string label) =>
            new SidebarActionLinkItem(label, controller, action, area ?? "");
    }

    private sealed class PageTarget(string page, string? area) : SidebarLinkTarget
    {
        internal override SidebarLinkItemBase CreateItem(string label) =>
            new SidebarPageLinkItem(label, page, area ?? "");
    }

    private sealed class UrlTarget(string url) : SidebarLinkTarget
    {
        internal override SidebarLinkItemBase CreateItem(string label) =>
            new SidebarLinkItem(label, url);
    }
}
