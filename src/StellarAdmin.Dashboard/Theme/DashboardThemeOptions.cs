namespace StellarAdmin.Dashboard;

/// <summary>
///     Options for the Dashboard theme.
/// </summary>
public sealed class DashboardThemeOptions
{
    /// <summary>
    ///     Whether to load the preset's suggested web fonts.
    /// </summary>
    /// <remarks>
    ///     Defaults to false.
    /// </remarks>
    public bool IncludeSuggestedFonts { get; set; }

    /// <summary>
    ///     The theme preset used by Dashboard pages.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DashboardThemePreset.Default" />.
    /// </remarks>
    public DashboardThemePreset Preset { get; set; } = DashboardThemePreset.Default;

    /// <summary>
    ///     An optional stylesheet of theme knobs, such as <c>~/css/theme.css</c>, loaded after the preset.
    /// </summary>
    public string? Stylesheet { get; set; }
}
