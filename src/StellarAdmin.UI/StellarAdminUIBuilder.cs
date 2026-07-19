using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI;

/// <summary>
/// Exposes the necessary methods required to configure the StellarAdmin UI services.
/// </summary>
public class StellarAdminUIBuilder
{
    /// <summary>
    ///     Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="StellarAdminUIBuilder" />.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminUIBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    ///     Adds a custom icon.
    /// </summary>
    /// <param name="name">The name of the icon.</param>
    /// <param name="iconDefinition">The icon definition.</param>
    /// <returns>The <see cref="StellarAdminUIBuilder" /> instance.</returns>
    public StellarAdminUIBuilder AddIcon(string name, IconDefinition iconDefinition)
    {
        DefaultIconManager.Instance.AddIcon(name, iconDefinition);

        return this;
    }

    /// <summary>
    ///     Registers a new icon pack.
    /// </summary>
    /// <typeparam name="TIconPack">The icon pack to register.</typeparam>
    /// <returns>The <see cref="StellarAdminUIBuilder" /> instance.</returns>
    public StellarAdminUIBuilder AddIconPack<TIconPack>()
        where TIconPack : IIconPack, new()
    {
        DefaultIconManager.Instance.AddIconPack<TIconPack>();

        return this;
    }

    /// <summary>
    ///     Configures the style pack used by the application.
    /// </summary>
    /// <typeparam name="TThemePack">The theme pack to use.</typeparam>
    /// <returns>The <see cref="StellarAdminUIBuilder" /> instance.</returns>
    public StellarAdminUIBuilder UseTheme<TThemePack>()
        where TThemePack : IThemePack, new()
    {
        ThemeManager.Instance.UseTheme<TThemePack>();

        return this;
    }

    /// <summary>
    ///     Configures the application-wide defaults for floating menu surfaces (color,
    ///     appearance and accent).
    /// </summary>
    /// <param name="configure">A callback for configuring <see cref="StellarAdminUIMenuOptions" />.</param>
    /// <returns>The <see cref="StellarAdminUIBuilder" /> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminUIBuilder ConfigureMenu(Action<StellarAdminUIMenuOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure<StellarAdminUIOptions>(options => configure(options.Menu));

        return this;
    }
}
