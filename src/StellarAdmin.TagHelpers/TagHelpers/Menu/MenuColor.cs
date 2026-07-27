namespace StellarAdmin.TagHelpers;

/// <summary>
///     The color scheme of a floating menu surface (Dropdown Menu content, and future menu
///     families).
/// </summary>
public enum MenuColor
{
    /// <summary>The menu renders in the current color scheme (opaque <c>popover</c> surface).</summary>
    Default,

    /// <summary>The menu surface renders in the dark color scheme regardless of the page scheme.</summary>
    Inverted,
}

internal static class MenuColorExtensions
{
    extension(MenuColor color)
    {
        /// <summary>
        ///     The literal class that renders this color, or <c>null</c> when no class is needed
        ///     (<see cref="MenuColor.Default" /> is the absence of one). Inverted is the
        ///     <c>dark</c> marker class: it re-scopes the theme variables for the surface and its
        ///     descendants, so it must be present on the element itself rather than styled from
        ///     the theme stylesheet.
        /// </summary>
        public string? GetSurfaceClass() =>
            color switch
            {
                MenuColor.Inverted => "dark",
                _ => null,
            };
    }
}
