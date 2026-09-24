using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard;

/// <summary>
///     Configures the Dashboard theme.
/// </summary>
public sealed class DashboardThemeBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Whether to load the selected theme's suggested web fonts.
    /// </summary>
    public bool IncludeSuggestedFonts
    {
        set =>
            _services.Configure<DashboardThemeOptions>(options =>
                options.IncludeSuggestedFonts = value
            );
    }

    /// <summary>
    ///     The theme used by Dashboard pages.
    /// </summary>
    public DashboardTheme Name
    {
        set => _services.Configure<DashboardThemeOptions>(options => options.Name = value);
    }

    internal DashboardThemeBuilder(IServiceCollection services) => _services = services;
}
