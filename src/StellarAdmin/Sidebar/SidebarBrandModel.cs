namespace StellarAdmin.Sidebar;

/// <summary>
///     The sidebar header's content, the model of <c>_SidebarBrand.cshtml</c>.
/// </summary>
public sealed class SidebarBrandModel
{
    /// <summary>
    ///     The URL the header links to.
    /// </summary>
    public string Href { get; }

    /// <summary>
    ///     The logo, or <c>null</c> for none.
    /// </summary>
    public BrandingLogo? Logo { get; }

    /// <summary>
    ///     The line under the title, or <c>null</c> for none.
    /// </summary>
    public string? Subtitle { get; }

    /// <summary>
    ///     The admin panel's name.
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
