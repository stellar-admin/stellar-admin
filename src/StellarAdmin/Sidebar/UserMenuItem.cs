namespace StellarAdmin.Sidebar;

/// <summary>
///     An entry in the sidebar's user menu: a link, a separator or the log out item.
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
    ///     Returns an item that logs the user out through the admin panel's logout endpoint.
    /// </summary>
    /// <remarks>
    ///     The endpoint signs out of the schemes configured with <see cref="UserMenuBuilder.Logout" />.
    ///     The item shows the <c>log-out</c> icon.
    /// </remarks>
    public static LogoutUserMenuItem Logout() => new(null);

    /// <summary>
    ///     Returns and configures an item that logs the user out through the admin panel's logout endpoint.
    /// </summary>
    /// <remarks>
    ///     The endpoint signs out of the schemes configured with <see cref="UserMenuBuilder.Logout" />.
    /// </remarks>
    /// <param name="configure">Configures the item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static LogoutUserMenuItem Logout(Action<UserMenuItemBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = Logout();
        configure(new(icon => item.Icon = icon));

        return item;
    }

    /// <summary>
    ///     Returns an item that logs the user out by posting to an endpoint of the application, such as
    ///     ASP.NET Core Identity's logout page.
    /// </summary>
    /// <remarks>
    ///     The item posts a form with an antiforgery token. The endpoint decides where to go afterwards.
    ///     For an endpoint that logs out on a GET request, use <see cref="Url(string, string)" /> instead.
    /// </remarks>
    /// <param name="path">An app-relative path such as <c>~/Identity/Account/Logout</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static LogoutUserMenuItem Logout(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new(path);
    }

    /// <summary>
    ///     Returns and configures an item that logs the user out by posting to an endpoint of the
    ///     application, such as ASP.NET Core Identity's logout page.
    /// </summary>
    /// <remarks>
    ///     The item posts a form with an antiforgery token. The endpoint decides where to go afterwards.
    ///     For an endpoint that logs out on a GET request, use <see cref="Url(string, string)" /> instead.
    /// </remarks>
    /// <param name="path">An app-relative path such as <c>~/Identity/Account/Logout</c>.</param>
    /// <param name="configure">Configures the item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static LogoutUserMenuItem Logout(string path, Action<UserMenuItemBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = Logout(path);
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
