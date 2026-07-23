namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The focus/highlight intensity of menu items (Dropdown Menu, and future menu families).
/// </summary>
public enum MenuAccent
{
    /// <summary>A low-contrast highlight (the muted <c>accent</c> color).</summary>
    Subtle,

    /// <summary>A solid highlight in the <c>primary</c> color.</summary>
    Bold,
}

internal static class MenuAccentExtensions
{
    extension(MenuAccent accent)
    {
        /// <summary>
        ///     The class name whose theme rule renders this accent, or <c>null</c> when none is
        ///     needed (<see cref="MenuAccent.Subtle" /> is the absence of a class — the default
        ///     <c>accent</c> highlight already applies).
        /// </summary>
        public string? GetSurfaceTokenName() =>
            accent switch
            {
                MenuAccent.Bold => "sa-menu-accent-bold",
                _ => null,
            };
    }
}
