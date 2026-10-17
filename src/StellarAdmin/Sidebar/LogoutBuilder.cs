namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the user menu's log out item and the logout endpoint it posts to.
/// </summary>
public sealed class LogoutBuilder
{
    private readonly LogoutOptions _options;

    /// <summary>
    ///     Sets the icon shown before the log out text.
    /// </summary>
    /// <value>
    ///     The name of the icon, or <see langword="null" /> for none. The default is
    ///     <c>log-out</c>.
    /// </value>
    public string? Icon
    {
        set => _options.Icon = value;
    }

    /// <summary>
    ///     Sets an application endpoint for the log out item to post to instead of the admin
    ///     panel's own.
    /// </summary>
    /// <value>
    ///     An app-relative path, such as <c>~/Identity/Account/Logout</c> for ASP.NET Core
    ///     Identity's logout page.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The value is empty or white space.
    /// </exception>
    /// <remarks>
    ///     The item posts a form with an antiforgery token. The endpoint decides where to go
    ///     afterwards, so the authentication schemes and <see cref="RedirectPath" /> don't apply.
    /// </remarks>
    public string Path
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.Path = value;
        }
    }

    /// <summary>
    ///     Sets where the admin panel's logout endpoint redirects after logging out.
    /// </summary>
    /// <value>
    ///     An app-relative path, such as <c>~/</c>. The default is the admin panel's home page.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The value is empty or white space.
    /// </exception>
    /// <remarks>
    ///     A remote scheme such as OpenID Connect returns here from the identity provider.
    /// </remarks>
    public string RedirectPath
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.RedirectPath = value;
        }
    }

    /// <summary>
    ///     Sets a value that indicates whether the user menu shows the log out item.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the user menu shows the log out item; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Set it to <see langword="false" /> when the application logs out somewhere else.
    /// </remarks>
    public bool Show
    {
        set => _options.Show = value;
    }

    internal LogoutBuilder(LogoutOptions options) => _options = options;

    /// <summary>
    ///     Adds an authentication scheme for the admin panel's logout endpoint to sign out of.
    /// </summary>
    /// <param name="scheme">The name of the authentication scheme.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="scheme" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="scheme" /> is empty or white space.
    /// </exception>
    /// <remarks>
    ///     The endpoint signs out of the schemes in the order they are added. Without schemes, it
    ///     signs out of the default authenticate scheme. Add local schemes such as cookies first,
    ///     and a remote scheme such as OpenID Connect last, since it redirects to the identity
    ///     provider.
    /// </remarks>
    public LogoutBuilder AddAuthenticationScheme(string scheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);

        _options.AuthenticationSchemes.Add(scheme);

        return this;
    }
}
