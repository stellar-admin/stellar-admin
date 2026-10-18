using System.Runtime.CompilerServices;

namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents the logo in the sidebar header.
/// </summary>
/// <remarks>
///     A logo is an icon or an image beside the title, or a wordmark image that replaces the
///     title.
/// </remarks>
public abstract class BrandingLogo
{
    private protected BrandingLogo() { }

    /// <summary>
    ///     Returns a logo that shows an icon in an accent tile beside the title.
    /// </summary>
    /// <param name="name">The name of the icon.</param>
    /// <returns>A logo that shows the icon.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="name" /> is empty or white space.
    /// </exception>
    public static IconBrandingLogo Icon(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new(name);
    }

    /// <summary>
    ///     Returns a logo that shows a square image beside the title.
    /// </summary>
    /// <param name="path">
    ///     The absolute URL or app-relative path of the image, such as <c>~/img/logo.svg</c>.
    /// </param>
    /// <returns>A logo that shows the image.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> is empty or white space.
    /// </exception>
    public static ImageBrandingLogo Image(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new(path, null);
    }

    /// <summary>
    ///     Returns a logo that shows a square image beside the title, with another image in dark
    ///     mode.
    /// </summary>
    /// <param name="path">
    ///     The absolute URL or app-relative path of the image, such as <c>~/img/logo.svg</c>.
    /// </param>
    /// <param name="darkPath">
    ///     The absolute URL or app-relative path of the image shown in dark mode.
    /// </param>
    /// <returns>A logo that shows the images.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> or <paramref name="darkPath" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> or <paramref name="darkPath" /> is empty or white space.
    /// </exception>
    public static ImageBrandingLogo Image(string path, string darkPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(darkPath);

        return new(path, darkPath);
    }

    /// <summary>
    ///     Returns a logo that shows a wordmark image in place of the title.
    /// </summary>
    /// <param name="path">
    ///     The absolute URL or app-relative path of the image, such as <c>~/img/wordmark.svg</c>.
    /// </param>
    /// <returns>A logo that shows the wordmark.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> is empty or white space.
    /// </exception>
    /// <remarks>
    ///     The wordmark is centered. The title becomes the image's alternative text. A wordmark
    ///     can't be combined with a subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new(path, null, WordmarkLayout.Center);
    }

    /// <summary>
    ///     Returns a logo that shows a wordmark image in place of the title, placed as specified.
    /// </summary>
    /// <param name="path">
    ///     The absolute URL or app-relative path of the image, such as <c>~/img/wordmark.svg</c>.
    /// </param>
    /// <param name="layout">How the wordmark is placed in the sidebar header.</param>
    /// <returns>A logo that shows the wordmark.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> is empty or white space.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="layout" /> isn't a defined value.
    /// </exception>
    /// <remarks>
    ///     The title becomes the image's alternative text. A wordmark can't be combined with a
    ///     subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path, WordmarkLayout layout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ThrowIfUndefined(layout);

        return new(path, null, layout);
    }

    /// <summary>
    ///     Returns a logo that shows a wordmark image in place of the title, with another image in
    ///     dark mode.
    /// </summary>
    /// <param name="path">
    ///     The absolute URL or app-relative path of the image, such as <c>~/img/wordmark.svg</c>.
    /// </param>
    /// <param name="darkPath">
    ///     The absolute URL or app-relative path of the image shown in dark mode.
    /// </param>
    /// <returns>A logo that shows the wordmarks.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> or <paramref name="darkPath" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> or <paramref name="darkPath" /> is empty or white space.
    /// </exception>
    /// <remarks>
    ///     The wordmark is centered. The title becomes the image's alternative text. A wordmark
    ///     can't be combined with a subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path, string darkPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(darkPath);

        return new(path, darkPath, WordmarkLayout.Center);
    }

    /// <summary>
    ///     Returns a logo that shows a wordmark image in place of the title, with another image in
    ///     dark mode, placed as specified.
    /// </summary>
    /// <param name="path">
    ///     The absolute URL or app-relative path of the image, such as <c>~/img/wordmark.svg</c>.
    /// </param>
    /// <param name="darkPath">
    ///     The absolute URL or app-relative path of the image shown in dark mode.
    /// </param>
    /// <param name="layout">How the wordmark is placed in the sidebar header.</param>
    /// <returns>A logo that shows the wordmarks.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> or <paramref name="darkPath" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> or <paramref name="darkPath" /> is empty or white space.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="layout" /> isn't a defined value.
    /// </exception>
    /// <remarks>
    ///     The title becomes the image's alternative text. A wordmark can't be combined with a
    ///     subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path, string darkPath, WordmarkLayout layout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(darkPath);
        ThrowIfUndefined(layout);

        return new(path, darkPath, layout);
    }

    private static void ThrowIfUndefined(
        WordmarkLayout layout,
        [CallerArgumentExpression(nameof(layout))] string? paramName = null
    )
    {
        if (!Enum.IsDefined(layout))
        {
            throw new ArgumentOutOfRangeException(paramName, layout, null);
        }
    }
}
