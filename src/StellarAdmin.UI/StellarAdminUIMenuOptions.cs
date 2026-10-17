namespace StellarAdmin.UI;

/// <summary>
///     Provides the look of dropdown menus across the application.
/// </summary>
/// <seealso cref="StellarAdminUIBuilder.ConfigureMenu" />
public sealed class StellarAdminUIMenuOptions
{
    /// <summary>
    ///     Gets or sets the color scheme of menus.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the color scheme. The default is
    ///     <see cref="MenuColor.Default" />.
    /// </value>
    public MenuColor Color { get; set; } = MenuColor.Default;

    /// <summary>
    ///     Gets or sets the surface of menus.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the surface. The default is
    ///     <see cref="MenuAppearance.Solid" />.
    /// </value>
    public MenuAppearance Appearance { get; set; } = MenuAppearance.Solid;

    /// <summary>
    ///     Gets or sets how strongly the highlighted menu item stands out.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the highlight. The default is
    ///     <see cref="MenuAccent.Subtle" />.
    /// </value>
    public MenuAccent Accent { get; set; } = MenuAccent.Subtle;
}
