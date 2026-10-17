using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Configures the StellarAdmin tag helpers, including icons, menus and forms.
/// </summary>
public sealed class StellarAdminUIBuilder
{
    /// <summary>
    ///     Gets the service collection that the builder configures.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StellarAdminUIBuilder" /> class.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="services" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminUIBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    ///     Adds an icon.
    /// </summary>
    /// <remarks>
    ///     The icon replaces any icon already registered with the same name. Names aren't case
    ///     sensitive.
    /// </remarks>
    /// <param name="name">The name that components use to render the icon.</param>
    /// <param name="iconDefinition">The icon to add.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    public StellarAdminUIBuilder AddIcon(string name, IconDefinition iconDefinition)
    {
        Services.Configure<IconOptions>(options => options.AddIcon(name, iconDefinition));

        return this;
    }

    /// <summary>
    ///     Adds the icons of an icon pack and its semantic icon mappings.
    /// </summary>
    /// <remarks>
    ///     The pack's icons replace any icons already registered with the same names.
    /// </remarks>
    /// <typeparam name="TIconPack">The type of the icon pack to add.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    public StellarAdminUIBuilder AddIconPack<TIconPack>()
        where TIconPack : IIconPack, new()
    {
        Services.Configure<IconOptions>(options => options.AddIconPack<TIconPack>());

        return this;
    }

    /// <summary>
    ///     Adds the icons of an icon pack, configured by a delegate.
    /// </summary>
    /// <remarks>
    ///     The pack's icons replace any icons already registered with the same names.
    /// </remarks>
    /// <typeparam name="TIconPack">The type of the icon pack to add.</typeparam>
    /// <param name="configure">A delegate that configures how the icon pack is added.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminUIBuilder AddIconPack<TIconPack>(Action<IconPackOptions> configure)
        where TIconPack : IIconPack, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure<IconOptions>(options => options.AddIconPack<TIconPack>(configure));

        return this;
    }

    /// <summary>
    ///     Removes all registered icons and semantic icon mappings.
    /// </summary>
    /// <remarks>
    ///     Only icons added before this call are removed.
    /// </remarks>
    /// <returns>The builder, so that calls can be chained.</returns>
    public StellarAdminUIBuilder ClearIcons()
    {
        Services.Configure<IconOptions>(options => options.ClearIcons());

        return this;
    }

    /// <summary>
    ///     Configures the defaults for forms across the application.
    /// </summary>
    /// <param name="configure">A delegate that configures the form defaults.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminUIBuilder ConfigureForms(Action<StellarAdminFormsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure<StellarAdminFormsOptions>(options =>
            configure(new StellarAdminFormsBuilder(options))
        );

        return this;
    }
}
