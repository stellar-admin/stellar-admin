namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the admin panel's logout endpoint, which the user menu's log out item posts to.
/// </summary>
public sealed class LogoutBuilder
{
    private readonly LogoutOptions _options;

    /// <summary>
    ///     The authentication schemes to sign out of, in order.
    /// </summary>
    /// <remarks>
    ///     When empty, the endpoint signs out of the default authenticate scheme, the one that signed the
    ///     user in. List local schemes such as cookies first and a remote scheme such as OpenID Connect
    ///     last, since its handler redirects to the identity provider.
    /// </remarks>
    public IList<string> AuthenticationSchemes => _options.AuthenticationSchemes;

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

    internal LogoutBuilder(LogoutOptions options) => _options = options;
}
