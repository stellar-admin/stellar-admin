namespace StellarAdmin.Dashboard;

/// <summary>
///     Options for the Dashboard theme.
/// </summary>
public sealed class DashboardThemeOptions
{
    /// <summary>
    ///     Whether to load the selected theme's suggested web fonts.
    /// </summary>
    /// <remarks>
    ///     Defaults to false.
    /// </remarks>
    public bool IncludeSuggestedFonts { get; set; }

    /// <summary>
    ///     The theme used by Dashboard pages.
    /// </summary>
    /// <remarks>
    ///     Defaults to shadcn Nova.
    /// </remarks>
    public DashboardTheme Name { get; set; } = DashboardTheme.ShadcnNova;
}
