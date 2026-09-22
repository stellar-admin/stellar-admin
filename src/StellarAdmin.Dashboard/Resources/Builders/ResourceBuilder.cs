using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource.
/// </summary>
public sealed class ResourceBuilder<TResource>
    : ResourceBuilderBase<TResource, ResourceBuilder<TResource>>
{
    internal ResourceBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    public ResourceBuilder<TResource> Index(Action<ResourceIndexBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(Services));

        return this;
    }

    /// <summary>
    ///     Specifies the data source used for the resource.
    /// </summary>
    public ResourceBuilder<TResource> UseDataSource<TDataSource>()
        where TDataSource : class, IResourceDataSource<TResource>
    {
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.DataSourceType = typeof(TDataSource)
        );
        Services.TryAddScoped<TDataSource>();
        Services.AddScoped<IResourceDataSource<TResource>>(services =>
            services.GetRequiredService<TDataSource>()
        );

        return this;
    }

    /// <summary>
    ///     Specifies the property that identifies a resource.
    /// </summary>
    public ResourceBuilder<TResource> UseKey<TKey>(Expression<Func<TResource, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (
            key.Body is not MemberExpression { Member: PropertyInfo property } member
            || member.Expression != key.Parameters[0]
            || property.GetMethod?.IsPublic != true
        )
        {
            throw new ArgumentException(
                "Select a direct property with a public getter.",
                nameof(key)
            );
        }

        var selector = key.Compile();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.KeySelector = resource =>
                Convert.ToString(selector(resource), CultureInfo.InvariantCulture)!;
            options.KeyPropertyName = property.Name;
        });

        return this;
    }
}
