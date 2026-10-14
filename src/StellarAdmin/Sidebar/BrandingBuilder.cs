namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the admin panel's name and the sidebar header.
/// </summary>
public sealed class BrandingBuilder
{
    private readonly BrandingOptions _options;

    /// <summary>
    ///     Where the sidebar header links to.
    /// </summary>
    /// <remarks>
    ///     An absolute URL, or an app-relative path such as <c>~/</c>. Defaults to the admin panel's home page.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public string Href
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.Href = value;
        }
    }

    /// <summary>
    ///     The logo in the sidebar header, or <c>null</c> for none.
    /// </summary>
    /// <remarks>
    ///     Create one with <see cref="BrandingLogo.Icon" />, <see cref="BrandingLogo.Image(string)" /> or
    ///     <see cref="BrandingLogo.Wordmark(string)" />. Defaults to <c>null</c>.
    /// </remarks>
    public BrandingLogo? Logo
    {
        set => _options.Logo = value;
    }

    /// <summary>
    ///     The line under the title in the sidebar header, or <c>null</c> for none.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>null</c>. A wordmark logo cannot be combined with a subtitle.
    /// </remarks>
    public string? Subtitle
    {
        set => _options.Subtitle = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    ///     The admin panel's name, shown in the sidebar header, the app header and the browser title.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application name made readable, e.g. "Voyager Admin" for <c>Voyager.Admin</c>,
    ///     or "Admin" when the application has no name.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
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
