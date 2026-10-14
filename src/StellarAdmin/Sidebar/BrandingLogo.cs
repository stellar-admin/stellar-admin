namespace StellarAdmin.Sidebar;

/// <summary>
///     The logo in the sidebar header: an icon, an image beside the title, or a wordmark that
///     replaces the title.
/// </summary>
public abstract class BrandingLogo
{
    private protected BrandingLogo() { }

    /// <summary>
    ///     Returns a logo that shows an icon in an accent tile beside the title.
    /// </summary>
    /// <param name="name">The icon name.</param>
    /// <exception cref="ArgumentException"></exception>
    public static IconBrandingLogo Icon(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new(name);
    }

    /// <summary>
    ///     Returns a logo that shows a square image beside the title.
    /// </summary>
    /// <param name="path">An absolute URL, or an app-relative path such as <c>~/img/logo.svg</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static ImageBrandingLogo Image(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new(path, null);
    }

    /// <summary>
    ///     Returns a logo that shows a square image beside the title, with another image in dark mode.
    /// </summary>
    /// <param name="path">An absolute URL, or an app-relative path such as <c>~/img/logo.svg</c>.</param>
    /// <param name="darkPath">The image shown in dark mode.</param>
    /// <exception cref="ArgumentException"></exception>
    public static ImageBrandingLogo Image(string path, string darkPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(darkPath);

        return new(path, darkPath);
    }

    /// <summary>
    ///     Returns a logo that shows a wordmark image in place of the title.
    /// </summary>
    /// <remarks>
    ///     The title remains the image's alternative text. A wordmark cannot be combined with a subtitle.
    /// </remarks>
    /// <param name="path">An absolute URL, or an app-relative path such as <c>~/img/wordmark.svg</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static WordmarkBrandingLogo Wordmark(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new(path, null);
    }

    /// <summary>
    ///     Returns a logo that shows a wordmark image in place of the title, with another image in dark mode.
    /// </summary>
    /// <remarks>
    ///     The title remains the image's alternative text. A wordmark cannot be combined with a subtitle.
    /// </remarks>
    /// <param name="path">An absolute URL, or an app-relative path such as <c>~/img/wordmark.svg</c>.</param>
    /// <param name="darkPath">The image shown in dark mode.</param>
    /// <exception cref="ArgumentException"></exception>
    public static WordmarkBrandingLogo Wordmark(string path, string darkPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(darkPath);

        return new(path, darkPath);
    }
}
