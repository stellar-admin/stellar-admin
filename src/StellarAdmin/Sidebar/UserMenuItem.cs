using StellarAdmin.Diagnostics;

namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents an item in the sidebar's user menu.
/// </summary>
/// <remarks>
///     An item is a link or a separator. Create items with the static methods of this class.
/// </remarks>
public abstract class UserMenuItem
{
    private protected UserMenuItem() { }

    // Why the value can't be used, when a factory method got a blank value
    internal ConfigurationValueProblem? Problem { get; init; }

    /// <summary>
    ///     Returns an item that links to an MVC controller action.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="action">The name of the action.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <returns>The item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="action" /> or <paramref name="controller" />
    ///     is <see langword="null" />.
    /// </exception>
    public static ActionUserMenuItem Action(string text, string action, string controller)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(controller);

        return new(text, action, controller, null)
        {
            Problem = ConfigurationValues.FindBlank(
                "UserMenuItem.Action",
                (nameof(text), text),
                (nameof(action), action),
                (nameof(controller), controller)
            ),
        };
    }

    /// <summary>
    ///     Returns an item that links to an MVC controller action in an area.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="action">The name of the action.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="area">The name of the area.</param>
    /// <returns>The item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="action" />, <paramref name="controller" /> or
    ///     <paramref name="area" /> is <see langword="null" />.
    /// </exception>
    public static ActionUserMenuItem Action(
        string text,
        string action,
        string controller,
        string area
    )
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(area);

        return new(text, action, controller, area)
        {
            Problem = ConfigurationValues.FindBlank(
                "UserMenuItem.Action",
                (nameof(text), text),
                (nameof(action), action),
                (nameof(controller), controller),
                (nameof(area), area)
            ),
        };
    }

    /// <summary>
    ///     Returns a configured item that links to an MVC controller action.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="action">The name of the action.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="configure">A delegate that configures the item.</param>
    /// <returns>The configured item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="action" />, <paramref name="controller" /> or
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public static ActionUserMenuItem Action(
        string text,
        string action,
        string controller,
        Action<UserMenuItemBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = Action(text, action, controller);
        configure(new(icon => item.Icon = icon));

        return item;
    }

    /// <summary>
    ///     Returns a configured item that links to an MVC controller action in an area.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="action">The name of the action.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="area">The name of the area.</param>
    /// <param name="configure">A delegate that configures the item.</param>
    /// <returns>The configured item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="action" />, <paramref name="controller" />,
    ///     <paramref name="area" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public static ActionUserMenuItem Action(
        string text,
        string action,
        string controller,
        string area,
        Action<UserMenuItemBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = Action(text, action, controller, area);
        configure(new(icon => item.Icon = icon));

        return item;
    }

    /// <summary>
    ///     Returns an item that links to a Razor Page.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="page">The name of the page, such as <c>/Account/Profile</c>.</param>
    /// <returns>The item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" /> or <paramref name="page" /> is <see langword="null" />.
    /// </exception>
    public static PageUserMenuItem Page(string text, string page)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(page);

        return new(text, page, null)
        {
            Problem = ConfigurationValues.FindBlank(
                "UserMenuItem.Page",
                (nameof(text), text),
                (nameof(page), page)
            ),
        };
    }

    /// <summary>
    ///     Returns a configured item that links to a Razor Page.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="page">The name of the page, such as <c>/Account/Profile</c>.</param>
    /// <param name="configure">A delegate that configures the item.</param>
    /// <returns>The configured item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="page" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    public static PageUserMenuItem Page(
        string text,
        string page,
        Action<UserMenuItemBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = Page(text, page);
        configure(new(icon => item.Icon = icon));

        return item;
    }

    /// <summary>
    ///     Returns an item that links to a Razor Page in an area.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="page">The name of the page, such as <c>/Account/Manage/Index</c>.</param>
    /// <param name="area">The name of the area, such as <c>Identity</c>.</param>
    /// <returns>The item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="page" /> or <paramref name="area" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" />, <paramref name="page" /> or <paramref name="area" /> is empty
    ///     or white space.
    /// </exception>
    public static PageUserMenuItem Page(string text, string page, string area)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(area);

        return new(text, page, area)
        {
            Problem = ConfigurationValues.FindBlank(
                "UserMenuItem.Page",
                (nameof(text), text),
                (nameof(page), page),
                (nameof(area), area)
            ),
        };
    }

    /// <summary>
    ///     Returns a configured item that links to a Razor Page in an area.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="page">The name of the page, such as <c>/Account/Manage/Index</c>.</param>
    /// <param name="area">The name of the area, such as <c>Identity</c>.</param>
    /// <param name="configure">A delegate that configures the item.</param>
    /// <returns>The configured item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="page" />, <paramref name="area" /> or
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" />, <paramref name="page" /> or <paramref name="area" /> is empty
    ///     or white space.
    /// </exception>
    public static PageUserMenuItem Page(
        string text,
        string page,
        string area,
        Action<UserMenuItemBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = Page(text, page, area);
        configure(new(icon => item.Icon = icon));

        return item;
    }

    /// <summary>
    ///     Returns a separator between groups of items.
    /// </summary>
    /// <returns>A separator.</returns>
    public static SeparatorUserMenuItem Separator() => new();

    /// <summary>
    ///     Returns an item that links to a URL.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="url">
    ///     The absolute URL or app-relative path to link to, such as <c>~/help</c>.
    /// </param>
    /// <returns>The item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" /> or <paramref name="url" /> is <see langword="null" />.
    /// </exception>
    public static UrlUserMenuItem Url(string text, string url)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(url);

        return new(text, url)
        {
            Problem = ConfigurationValues.FindBlank(
                "UserMenuItem.Url",
                (nameof(text), text),
                (nameof(url), url)
            ),
        };
    }

    /// <summary>
    ///     Returns a configured item that links to a URL.
    /// </summary>
    /// <param name="text">The text of the item.</param>
    /// <param name="url">
    ///     The absolute URL or app-relative path to link to, such as <c>~/help</c>.
    /// </param>
    /// <param name="configure">A delegate that configures the item.</param>
    /// <returns>The configured item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" />, <paramref name="url" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    public static UrlUserMenuItem Url(
        string text,
        string url,
        Action<UserMenuItemBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = Url(text, url);
        configure(new(icon => item.Icon = icon));

        return item;
    }
}
