using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource.
/// </summary>
public sealed class ResourceBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The plural resource label.
    /// </summary>
    public string PluralLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options => options.PluralLabel = value);
    }

    /// <summary>
    ///     The singular resource label.
    /// </summary>
    public string SingularLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SingularLabel = value
            );
    }

    internal ResourceBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    public ResourceBuilder<TResource> Index(Action<ResourceIndexBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Specifies the data source used for the resource.
    /// </summary>
    public ResourceBuilder<TResource> UseDataSource<TDataSource>()
        where TDataSource : class, IResourceDataSource<TResource>
    {
        _services.TryAddScoped<TDataSource>();
        _services.AddScoped<IResourceDataSource<TResource>>(services =>
            services.GetRequiredService<TDataSource>()
        );

        return this;
    }
}
