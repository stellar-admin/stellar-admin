namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the admin panel's name and the sidebar header.
/// </summary>
public sealed class BrandingBuilder
{
    private readonly BrandingOptions _options;

    /// <summary>
    ///     Sets the URL that the sidebar header links to.
    /// </summary>
    /// <value>
    ///     An absolute URL, or an app-relative path such as <c>~/</c>. The default is the admin
    ///     panel's home page.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The value is empty or white space.
    /// </exception>
    public string Href
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.Href = value;
        }
    }

    /// <summary>
    ///     Sets the logo in the sidebar header.
    /// </summary>
    /// <value>
    ///     The logo, or <see langword="null" /> for none. The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Create a logo with <see cref="BrandingLogo.Icon" />,
    ///     <see cref="BrandingLogo.Image(string)" /> or
    ///     <see cref="BrandingLogo.Wordmark(string)" />.
    /// </remarks>
    public BrandingLogo? Logo
    {
        set => _options.Logo = value;
    }

    /// <summary>
    ///     Sets the line under the title in the sidebar header.
    /// </summary>
    /// <value>
    ///     The subtitle, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     An empty or white-space value shows no subtitle. A wordmark logo can't be combined with
    ///     a subtitle.
    /// </remarks>
    public string? Subtitle
    {
        set => _options.Subtitle = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    ///     Sets the admin panel's name.
    /// </summary>
    /// <value>
    ///     The name shown in the sidebar header, the app header and the browser title. The default
    ///     is the application name made readable, such as "Voyager Admin" for
    ///     <c>Voyager.Admin</c>, or "Admin" when the application has no name.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The value is empty or white space.
    /// </exception>
    public string Title
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.Title = value;
        }
    }

    internal BrandingBuilder(BrandingOptions options) => _options = options;
}
