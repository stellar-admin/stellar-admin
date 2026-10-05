using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default page text for resources.
/// </summary>
public sealed class ResourceLabelsBuilder
{
    private readonly IServiceCollection _services;

    internal ResourceLabelsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures default create page text.
    /// </summary>
    public ResourceLabelsBuilder Create(Action<ResourceCreateLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures default delete confirmation text.
    /// </summary>
    public ResourceLabelsBuilder Delete(Action<ResourceDeleteLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures default edit page text.
    /// </summary>
    public ResourceLabelsBuilder Edit(Action<ResourceEditLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures default index page text.
    /// </summary>
    public ResourceLabelsBuilder Index(Action<ResourceIndexLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures default lookup editor text.
    /// </summary>
    public ResourceLabelsBuilder Lookup(Action<LookupLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures default text for the sheet that pages load content into.
    /// </summary>
    public ResourceLabelsBuilder Sheet(Action<ResourceSheetLabelsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }
}
