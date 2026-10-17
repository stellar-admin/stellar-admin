namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents a logo that shows a square image beside the title.
/// </summary>
public sealed class ImageBrandingLogo : BrandingLogo
{
    /// <summary>
    ///     Gets the URL or app-relative path of the image shown in dark mode, or
    ///     <see langword="null" /> to show <see cref="Path" /> in both modes.
    /// </summary>
    public string? DarkPath { get; }

    /// <summary>
    ///     Gets the URL or app-relative path of the image.
    /// </summary>
    public string Path { get; }

    internal ImageBrandingLogo(string path, string? darkPath)
    {
        Path = path;
        DarkPath = darkPath;
    }
}
