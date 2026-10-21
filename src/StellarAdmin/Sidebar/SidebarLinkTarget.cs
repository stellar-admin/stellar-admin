using StellarAdmin.Diagnostics;

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

    // Why the value can't be used, when a factory method got a blank value
    internal ConfigurationValueProblem? Problem { get; init; }

    /// <summary>
    ///     Returns a target for an MVC controller action outside any area.
    /// </summary>
    /// <param name="action">The name of the action.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="action" /> or <paramref name="controller" /> is <see langword="null" />.
    /// </exception>
    public static SidebarLinkTarget Action(string action, string controller)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(controller);

        return new ActionTarget(action, controller, null)
        {
            Problem = ConfigurationValues.FindBlank(
                "SidebarLinkTarget.Action",
                (nameof(action), action),
                (nameof(controller), controller)
            ),
        };
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
    public static SidebarLinkTarget Action(string action, string controller, string area)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(area);

        return new ActionTarget(action, controller, area)
        {
            Problem = ConfigurationValues.FindBlank(
                "SidebarLinkTarget.Action",
                (nameof(action), action),
                (nameof(controller), controller),
                (nameof(area), area)
            ),
        };
    }

    /// <summary>
    ///     Returns a target for a Razor Page outside any area.
    /// </summary>
    /// <param name="page">The name of the page, such as <c>/Reports/Index</c>.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="page" /> is <see langword="null" />.
    /// </exception>
    public static SidebarLinkTarget Page(string page)
    {
        ArgumentNullException.ThrowIfNull(page);

        return new PageTarget(page, null)
        {
            Problem = ConfigurationValues.FindBlank("SidebarLinkTarget.Page", (nameof(page), page)),
        };
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
    public static SidebarLinkTarget Page(string page, string area)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(area);

        return new PageTarget(page, area)
        {
            Problem = ConfigurationValues.FindBlank(
                "SidebarLinkTarget.Page",
                (nameof(page), page),
                (nameof(area), area)
            ),
        };
    }

    /// <summary>
    ///     Returns a target for a URL.
    /// </summary>
    /// <param name="url">The absolute URL or app-relative path, such as <c>~/reports</c>.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="url" /> is <see langword="null" />.
    /// </exception>
    public static SidebarLinkTarget Url(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        return new UrlTarget(url)
        {
            Problem = ConfigurationValues.FindBlank("SidebarLinkTarget.Url", (nameof(url), url)),
        };
    }

    internal abstract SidebarLinkItemBase CreateItem(SidebarLinkOptions link);

    // Admin panel pages render in the StellarAdmin area, which link generation keeps as an ambient
    // value unless the link names another area. An empty area links outside any area.
    private sealed class ActionTarget(string action, string controller, string? area)
        : SidebarLinkTarget
    {
        internal override SidebarLinkItemBase CreateItem(SidebarLinkOptions link) =>
            new SidebarActionLinkItem(link.Label, controller, action, area ?? "")
            {
                Icon = link.Icon,
                OpenInNewTab = link.OpenInNewTab,
                Order = link.Order,
                Position = link.Position,
            };
    }

    private sealed class PageTarget(string page, string? area) : SidebarLinkTarget
    {
        internal override SidebarLinkItemBase CreateItem(SidebarLinkOptions link) =>
            new SidebarPageLinkItem(link.Label, page, area ?? "")
            {
                Icon = link.Icon,
                OpenInNewTab = link.OpenInNewTab,
                Order = link.Order,
                Position = link.Position,
            };
    }

    private sealed class UrlTarget(string url) : SidebarLinkTarget
    {
        internal override SidebarLinkItemBase CreateItem(SidebarLinkOptions link) =>
            new SidebarLinkItem(link.Label, url)
            {
                Icon = link.Icon,
                OpenInNewTab = link.OpenInNewTab,
                Order = link.Order,
                Position = link.Position,
            };
    }
}
