using StellarAdmin.UI.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Application-wide settings for floating menu surfaces (Dropdown Menu, and future menu
///     families). Configured via <see cref="Builders.StellarAdminBuilder.ConfigureMenu" />.
/// </summary>
public class StellarAdminUIMenuOptions
{
    /// <summary>The color scheme of menu surfaces. Defaults to <see cref="MenuColor.Default" />.</summary>
    public MenuColor Color { get; set; } = MenuColor.Default;

    /// <summary>The surface treatment of menus. Defaults to <see cref="MenuAppearance.Solid" />.</summary>
    public MenuAppearance Appearance { get; set; } = MenuAppearance.Solid;

    /// <summary>The item highlight intensity. Defaults to <see cref="MenuAccent.Subtle" />.</summary>
    public MenuAccent Accent { get; set; } = MenuAccent.Subtle;
}
