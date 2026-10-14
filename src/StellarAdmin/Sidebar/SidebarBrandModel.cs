namespace StellarAdmin.Sidebar;

/// <summary>
///     The sidebar header's content, the model of <c>_SidebarBrand.cshtml</c>.
/// </summary>
/// <param name="Title">The admin panel's name.</param>
/// <param name="Subtitle">The line under the title, or <c>null</c> for none.</param>
/// <param name="Logo">The logo, or <c>null</c> for none.</param>
/// <param name="Href">The URL the header links to.</param>
public sealed record SidebarBrandModel(
    string Title,
    string? Subtitle,
    BrandingLogo? Logo,
    string Href
);
