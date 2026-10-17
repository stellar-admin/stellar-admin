namespace StellarAdmin.UI;

/// <summary>
///     Specifies how strongly menu items are highlighted when focused.
/// </summary>
public enum MenuAccent
{
    /// <summary>
    ///     A low-contrast highlight tinted by the accent color.
    /// </summary>
    Subtle,

    /// <summary>
    ///     A solid highlight in the accent color.
    /// </summary>
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
