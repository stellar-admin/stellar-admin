namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents a logo that shows a wordmark image in place of the title.
/// </summary>
public sealed class WordmarkBrandingLogo : BrandingLogo
{
    /// <summary>
    ///     Gets the URL or app-relative path of the image shown in dark mode, or
    ///     <see langword="null" /> to show <see cref="Path" /> in both modes.
    /// </summary>
    public string? DarkPath { get; }

    /// <summary>
    ///     Gets how the wordmark is placed in the sidebar header.
    /// </summary>
    public WordmarkLayout Layout { get; }

    /// <summary>
    ///     Gets the URL or app-relative path of the image.
    /// </summary>
    public string Path { get; }

    internal WordmarkBrandingLogo(string path, string? darkPath, WordmarkLayout layout)
    {
        Path = path;
        DarkPath = darkPath;
        Layout = layout;
    }
}
