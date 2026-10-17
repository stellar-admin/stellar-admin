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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" />, <paramref name="action" /> or <paramref name="controller" />
    ///     is empty or white space.
    /// </exception>
    public static ActionUserMenuItem Action(string text, string action, string controller)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);

        return new(text, action, controller, null);
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" />, <paramref name="action" />, <paramref name="controller" /> or
    ///     <paramref name="area" /> is empty or white space.
    /// </exception>
    public static ActionUserMenuItem Action(
        string text,
        string action,
        string controller,
        string area
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(area);

        return new(text, action, controller, area);
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" />, <paramref name="action" /> or <paramref name="controller" />
    ///     is empty or white space.
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" />, <paramref name="action" />, <paramref name="controller" /> or
    ///     <paramref name="area" /> is empty or white space.
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" /> or <paramref name="page" /> is empty or white space.
    /// </exception>
    public static PageUserMenuItem Page(string text, string page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(page);

        return new(text, page, null);
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" /> or <paramref name="page" /> is empty or white space.
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
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(page);
        ArgumentException.ThrowIfNullOrWhiteSpace(area);

        return new(text, page, area);
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" /> or <paramref name="url" /> is empty or white space.
    /// </exception>
    public static UrlUserMenuItem Url(string text, string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new(text, url);
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="text" /> or <paramref name="url" /> is empty or white space.
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
