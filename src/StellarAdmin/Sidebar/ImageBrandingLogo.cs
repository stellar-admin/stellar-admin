namespace StellarAdmin.Sidebar;

/// <summary>
///     A logo that shows a square image beside the title.
/// </summary>
public sealed class ImageBrandingLogo : BrandingLogo
{
    /// <summary>
    ///     The image shown in dark mode, or <c>null</c> to show <see cref="Path" /> in both modes.
    /// </summary>
    public string? DarkPath { get; }

    /// <summary>
    ///     The image's URL or app-relative path.
    /// </summary>
    public string Path { get; }

    internal ImageBrandingLogo(string path, string? darkPath)
    {
        Path = path;
        DarkPath = darkPath;
    }
}
