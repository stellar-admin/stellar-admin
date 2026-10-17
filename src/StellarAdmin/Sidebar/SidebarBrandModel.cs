namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents the content of the sidebar header, for the <c>_SidebarBrand.cshtml</c> view.
/// </summary>
public sealed class SidebarBrandModel
{
    /// <summary>
    ///     Gets the URL that the header links to.
    /// </summary>
    public string Href { get; }

    /// <summary>
    ///     Gets the logo, or <see langword="null" /> for none.
    /// </summary>
    public BrandingLogo? Logo { get; }

    /// <summary>
    ///     Gets the line under the title, or <see langword="null" /> for none.
    /// </summary>
    public string? Subtitle { get; }

    /// <summary>
    ///     Gets the admin panel's name.
    /// </summary>
    public string Title { get; }

    internal SidebarBrandModel(string title, string? subtitle, BrandingLogo? logo, string href)
    {
        Title = title;
        Subtitle = subtitle;
        Logo = logo;
        Href = href;
    }
}
