using StellarAdmin.Diagnostics;

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

    // Why the logo can't be shown, when a factory method got a blank path or an undefined layout
    internal string? Problem { get; init; }

    /// <summary>
    ///     Returns a logo that shows an icon in an accent tile beside the title.
    /// </summary>
    /// <param name="name">The name of the icon.</param>
    /// <returns>A logo that shows the icon.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    public static IconBrandingLogo Icon(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return new(name) { Problem = ConfigurationValues.FindBlank((nameof(name), name)) };
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
    public static ImageBrandingLogo Image(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return new(path, null) { Problem = ConfigurationValues.FindBlank((nameof(path), path)) };
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
    public static ImageBrandingLogo Image(string path, string darkPath)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(darkPath);

        return new(path, darkPath)
        {
            Problem = ConfigurationValues.FindBlank(
                (nameof(path), path),
                (nameof(darkPath), darkPath)
            ),
        };
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
    /// <remarks>
    ///     The wordmark is centered. The title becomes the image's alternative text. A wordmark
    ///     can't be combined with a subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return new(path, null, WordmarkLayout.Center)
        {
            Problem = ConfigurationValues.FindBlank((nameof(path), path)),
        };
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
    /// <remarks>
    ///     The title becomes the image's alternative text. A wordmark can't be combined with a
    ///     subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path, WordmarkLayout layout)
    {
        ArgumentNullException.ThrowIfNull(path);

        return new(path, null, layout)
        {
            Problem = ConfigurationValues.FindBlank((nameof(path), path)) ?? FindUndefined(layout),
        };
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
    /// <remarks>
    ///     The wordmark is centered. The title becomes the image's alternative text. A wordmark
    ///     can't be combined with a subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path, string darkPath)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(darkPath);

        return new(path, darkPath, WordmarkLayout.Center)
        {
            Problem = ConfigurationValues.FindBlank(
                (nameof(path), path),
                (nameof(darkPath), darkPath)
            ),
        };
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
    /// <remarks>
    ///     The title becomes the image's alternative text. A wordmark can't be combined with a
    ///     subtitle.
    /// </remarks>
    public static WordmarkBrandingLogo Wordmark(string path, string darkPath, WordmarkLayout layout)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(darkPath);

        return new(path, darkPath, layout)
        {
            Problem =
                ConfigurationValues.FindBlank((nameof(path), path), (nameof(darkPath), darkPath))
                ?? FindUndefined(layout),
        };
    }

    private static string? FindUndefined(WordmarkLayout layout) =>
        Enum.IsDefined(layout) ? null : $"Its layout {(int)layout} isn't a WordmarkLayout value.";
}
