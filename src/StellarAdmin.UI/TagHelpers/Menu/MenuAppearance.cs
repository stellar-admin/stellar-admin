namespace StellarAdmin.UI;

/// <summary>
///     Specifies the surface style of a floating menu.
/// </summary>
public enum MenuAppearance
{
    /// <summary>
    ///     An opaque surface.
    /// </summary>
    Solid,

    /// <summary>
    ///     A frosted glass surface that blurs the page behind a translucent background.
    /// </summary>
    Translucent,
}

internal static class MenuAppearanceExtensions
{
    extension(MenuAppearance appearance)
    {
        /// <summary>
        ///     The class name whose theme rule renders this appearance, or <c>null</c> when none is
        ///     needed (<see cref="MenuAppearance.Solid" /> is the absence of a class).
        /// </summary>
        public string? GetSurfaceTokenName() =>
            appearance switch
            {
                MenuAppearance.Translucent => "sa-menu-translucent",
                _ => null,
            };
    }
}
