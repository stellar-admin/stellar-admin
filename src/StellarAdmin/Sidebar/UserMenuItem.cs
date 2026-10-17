namespace StellarAdmin.Sidebar;

/// <summary>
///     An entry in the sidebar's user menu: a link or a separator.
/// </summary>
public abstract class UserMenuItem
{
    private protected UserMenuItem() { }

    /// <summary>
    ///     Returns an item that links to an MVC controller action.
    /// </summary>
    /// <param name="text">The item text.</param>
    /// <param name="action">The action name.</param>
    /// <param name="controller">The controller name.</param>
    /// <exception cref="ArgumentException"></exception>
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
    /// <param name="text">The item text.</param>
    /// <param name="action">The action name.</param>
    /// <param name="controller">The controller name.</param>
    /// <param name="area">The area name.</param>
    /// <exception cref="ArgumentException"></exception>
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
    ///     Returns and configures an item that links to an MVC controller action.
    /// </summary>
    /// <param name="text">The item text.</param>
    /// <param name="action">The action name.</param>
    /// <param name="controller">The controller name.</param>
    /// <param name="configure">Configures the item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
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
    ///     Returns and configures an item that links to an MVC controller action in an area.
    /// </summary>
    /// <param name="text">The item text.</param>
    /// <param name="action">The action name.</param>
    /// <param name="controller">The controller name.</param>
    /// <param name="area">The area name.</param>
    /// <param name="configure">Configures the item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
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
    /// <param name="text">The item text.</param>
    /// <param name="page">The page name, e.g. <c>/Account/Profile</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static PageUserMenuItem Page(string text, string page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(page);

        return new(text, page, null);
    }

    /// <summary>
    ///     Returns and configures an item that links to a Razor Page.
    /// </summary>
    /// <param name="text">The item text.</param>
    /// <param name="page">The page name, e.g. <c>/Account/Profile</c>.</param>
    /// <param name="configure">Configures the item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
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
    /// <param name="text">The item text.</param>
    /// <param name="page">The page name, e.g. <c>/Account/Manage/Index</c>.</param>
    /// <param name="area">The area name, e.g. <c>Identity</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static PageUserMenuItem Page(string text, string page, string area)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(page);
        ArgumentException.ThrowIfNullOrWhiteSpace(area);

        return new(text, page, area);
    }

    /// <summary>
    ///     Returns and configures an item that links to a Razor Page in an area.
    /// </summary>
    /// <param name="text">The item text.</param>
    /// <param name="page">The page name, e.g. <c>/Account/Manage/Index</c>.</param>
    /// <param name="area">The area name, e.g. <c>Identity</c>.</param>
    /// <param name="configure">Configures the item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
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
    public static SeparatorUserMenuItem Separator() => new();

    /// <summary>
    ///     Returns an item that links to a URL.
    /// </summary>
    /// <param name="text">The item text.</param>
    /// <param name="url">An absolute URL, or an app-relative path such as <c>~/help</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static UrlUserMenuItem Url(string text, string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new(text, url);
    }

    /// <summary>
    ///     Returns and configures an item that links to a URL.
    /// </summary>
    /// <param name="text">The item text.</param>
    /// <param name="url">An absolute URL, or an app-relative path such as <c>~/help</c>.</param>
    /// <param name="configure">Configures the item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
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
