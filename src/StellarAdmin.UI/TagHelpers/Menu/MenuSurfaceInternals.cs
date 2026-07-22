namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Shared composition for floating menu surfaces (Dropdown Menu content/sub-content, and
///     future Context Menu / Menubar / Navigation Menu families). Turns the resolved
///     <see cref="MenuColor" /> / <see cref="MenuAppearance" /> / <see cref="MenuAccent" />
///     settings into the theme tokens applied to a menu content element.
/// </summary>
/// <remarks>
///     Each accessor returns <c>null</c> when the setting needs no token (Default color, Solid
///     appearance, Subtle accent); <see cref="ICssClassMerger.Merge" /> skips <c>null</c>
///     elements, so the results can be passed straight into a <c>Merge(...)</c> call.
/// </remarks>
internal static class MenuSurfaceInternals
{
    public static string? ColorToken(MenuColor color) => color.GetSurfaceClass();

    public static string? AppearanceToken(MenuAppearance appearance) =>
        appearance.GetSurfaceTokenName();

    public static string? AccentToken(MenuAccent accent) => accent.GetSurfaceTokenName();
}
