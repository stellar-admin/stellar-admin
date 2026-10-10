using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard;

/// <summary>
///     Configures the Dashboard theme.
/// </summary>
public sealed class DashboardThemeBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Whether to load the preset's suggested web fonts.
    /// </summary>
    public bool IncludeSuggestedFonts
    {
        set =>
            _services.Configure<DashboardThemeOptions>(options =>
                options.IncludeSuggestedFonts = value
            );
    }

    /// <summary>
    ///     The theme preset used by Dashboard pages.
    /// </summary>
    public DashboardThemePreset Preset
    {
        set => _services.Configure<DashboardThemeOptions>(options => options.Preset = value);
    }

    /// <summary>
    ///     An optional stylesheet of theme knobs, such as <c>~/css/theme.css</c>, loaded after the preset.
    /// </summary>
    public string? Stylesheet
    {
        set => _services.Configure<DashboardThemeOptions>(options => options.Stylesheet = value);
    }

    internal DashboardThemeBuilder(IServiceCollection services) => _services = services;
}
