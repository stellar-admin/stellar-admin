namespace StellarAdmin.Dashboard.Sidebar;

/// <summary>
///     The destination of a sidebar link.
/// </summary>
public abstract class SidebarLinkTarget
{
    private protected SidebarLinkTarget() { }

    /// <summary>
    ///     Returns a target for an MVC controller action.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <param name="controller">The controller name.</param>
    /// <param name="area">The area name, or <c>null</c> for an action outside any area.</param>
    /// <exception cref="ArgumentException"></exception>
    public static SidebarLinkTarget Action(string action, string controller, string? area = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);
        ValidateArea(area);

        return new ActionTarget(action, controller, area);
    }

    /// <summary>
    ///     Returns a target for a Razor Page.
    /// </summary>
    /// <param name="page">The page name, e.g. <c>/Reports/Index</c>.</param>
    /// <param name="area">The area name, or <c>null</c> for a page outside any area.</param>
    /// <exception cref="ArgumentException"></exception>
    public static SidebarLinkTarget Page(string page, string? area = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(page);
        ValidateArea(area);

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

    private static void ValidateArea(string? area)
    {
        if (area is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(area);
        }
    }

    internal abstract SidebarLinkItemBase CreateItem(string label);

    // Dashboard pages render in the StellarAdmin area, which link generation keeps as an ambient
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
