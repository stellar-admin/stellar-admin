namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the user menu's log out item and the admin panel's logout endpoint it posts to.
/// </summary>
public sealed class LogoutBuilder
{
    private readonly LogoutOptions _options;

    /// <summary>
    ///     The icon shown before the log out text, or <c>null</c> for none.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>log-out</c>.
    /// </remarks>
    public string? Icon
    {
        set => _options.Icon = value;
    }

    /// <summary>
    ///     An endpoint of the application for the log out item to post to instead of the admin panel's, such as
    ///     ASP.NET Core Identity's logout page.
    /// </summary>
    /// <remarks>
    ///     An app-relative path such as <c>~/Identity/Account/Logout</c>. The item posts a form with an antiforgery
    ///     token, and the endpoint decides where to go afterwards, so the authentication schemes and
    ///     <see cref="RedirectPath" /> don't apply.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public string Path
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.Path = value;
        }
    }

    /// <summary>
    ///     Where the endpoint redirects after logging out, unless a scheme's handler redirected.
    /// </summary>
    /// <remarks>
    ///     An app-relative path such as <c>~/</c>. Defaults to the admin panel's home page. A remote scheme
    ///     such as OpenID Connect returns here from the identity provider.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public string RedirectPath
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.RedirectPath = value;
        }
    }

    /// <summary>
    ///     Whether the user menu shows the log out item.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>. Set it to <c>false</c> when the application logs out somewhere else.
    /// </remarks>
    public bool Show
    {
        set => _options.Show = value;
    }

    internal LogoutBuilder(LogoutOptions options) => _options = options;

    /// <summary>
    ///     Adds an authentication scheme for the admin panel's logout endpoint to sign out of. Schemes are signed
    ///     out of in the order they are added.
    /// </summary>
    /// <remarks>
    ///     Without schemes, the endpoint signs out of the default authenticate scheme, the one that signed the
    ///     user in. Add local schemes such as cookies first and a remote scheme such as OpenID Connect last,
    ///     since its handler redirects to the identity provider.
    /// </remarks>
    /// <param name="scheme">The authentication scheme name.</param>
    /// <exception cref="ArgumentException"></exception>
    public LogoutBuilder AddAuthenticationScheme(string scheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);

        _options.AuthenticationSchemes.Add(scheme);

        return this;
    }
}
