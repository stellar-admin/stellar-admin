namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents the destination of a sidebar link.
/// </summary>
/// <remarks>
///     Create targets with the static methods of this class.
/// </remarks>
public abstract class SidebarLinkTarget
{
    private protected SidebarLinkTarget() { }

    /// <summary>
    ///     Returns a target for an MVC controller action outside any area.
    /// </summary>
    /// <param name="action">The name of the action.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="action" /> or <paramref name="controller" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="action" /> or <paramref name="controller" /> is empty or white space.
    /// </exception>
    public static SidebarLinkTarget Action(string action, string controller)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);

        return new ActionTarget(action, controller, null);
    }

    /// <summary>
    ///     Returns a target for an MVC controller action in an area.
    /// </summary>
    /// <param name="action">The name of the action.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="area">The name of the area.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="action" />, <paramref name="controller" /> or <paramref name="area" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="action" />, <paramref name="controller" /> or <paramref name="area" />
    ///     is empty or white space.
    /// </exception>
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
    /// <param name="page">The name of the page, such as <c>/Reports/Index</c>.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="page" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="page" /> is empty or white space.
    /// </exception>
    public static SidebarLinkTarget Page(string page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(page);

        return new PageTarget(page, null);
    }

    /// <summary>
    ///     Returns a target for a Razor Page in an area.
    /// </summary>
    /// <param name="page">The name of the page, such as <c>/Reports/Index</c>.</param>
    /// <param name="area">The name of the area.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="page" /> or <paramref name="area" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="page" /> or <paramref name="area" /> is empty or white space.
    /// </exception>
    public static SidebarLinkTarget Page(string page, string area)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(page);
        ArgumentException.ThrowIfNullOrWhiteSpace(area);

        return new PageTarget(page, area);
    }

    /// <summary>
    ///     Returns a target for a URL.
    /// </summary>
    /// <param name="url">The absolute URL or app-relative path, such as <c>~/reports</c>.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="url" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="url" /> is empty or white space.
    /// </exception>
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
