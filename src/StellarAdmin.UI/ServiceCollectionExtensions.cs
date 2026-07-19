using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.UI.Builders;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Theming;
using TailwindMerge;

namespace StellarAdmin.UI;

/// <summary>
///     Extensions for registering and configuring the StellarAdmin.UI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Provides the common entry point for registering the StellarAdmin.UI services.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>The <see cref="StellarAdminBuilder" /> instances that allows you to register and configure StellarAdmin.UI services.</returns>
    public static StellarAdminBuilder AddStellarAdmin(this IServiceCollection services)
    {
        services.AddOptions<StellarAdminOptions>();

        services
            .AddSingleton<TwMerge>()
            .AddSingleton<ICssClassMerger, DefaultCssClassMerger>()
            .AddSingleton<IIconManager>(_ => DefaultIconManager.Instance)
            .AddSingleton<ThemeManager>(_ => ThemeManager.Instance);

        var builder = new StellarAdminBuilder(services);
        builder.AddIconPack<LucideIconPack>();
        builder.UseTheme<VegaThemePack>();

        return builder;
    }
}
