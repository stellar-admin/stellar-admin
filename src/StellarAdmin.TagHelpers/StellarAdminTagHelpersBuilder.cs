using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
/// Exposes the necessary methods required to configure the StellarAdmin tag helper services.
/// </summary>
public class StellarAdminTagHelpersBuilder
{
    /// <summary>
    ///     Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="StellarAdminTagHelpersBuilder" />.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminTagHelpersBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    ///     Adds a custom icon.
    /// </summary>
    /// <param name="name">The name of the icon.</param>
    /// <param name="iconDefinition">The icon definition.</param>
    /// <returns>The <see cref="StellarAdminTagHelpersBuilder" /> instance.</returns>
    public StellarAdminTagHelpersBuilder AddIcon(string name, IconDefinition iconDefinition)
    {
        DefaultIconManager.Instance.AddIcon(name, iconDefinition);

        return this;
    }

    /// <summary>
    ///     Registers a new icon pack.
    /// </summary>
    /// <typeparam name="TIconPack">The icon pack to register.</typeparam>
    /// <returns>The <see cref="StellarAdminTagHelpersBuilder" /> instance.</returns>
    public StellarAdminTagHelpersBuilder AddIconPack<TIconPack>()
        where TIconPack : IIconPack, new()
    {
        DefaultIconManager.Instance.AddIconPack<TIconPack>();

        return this;
    }

    /// <summary>
    ///     Configures the application-wide defaults for floating menu surfaces (color,
    ///     appearance and accent).
    /// </summary>
    /// <param name="configure">A callback for configuring <see cref="StellarAdminTagHelpersMenuOptions" />.</param>
    /// <returns>The <see cref="StellarAdminTagHelpersBuilder" /> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminTagHelpersBuilder ConfigureMenu(Action<StellarAdminTagHelpersMenuOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure<StellarAdminTagHelpersOptions>(options => configure(options.Menu));

        return this;
    }
}
