using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceBuilder<TResource>
    : ResourceBuilderBase<TResource, ResourceBuilder<TResource>>
{
    internal ResourceBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    /// <param name="configure">A delegate that configures the index page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceBuilder<TResource> Index(Action<ResourceIndexBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(Services));

        return this;
    }

    /// <summary>
    ///     Sets the data source that loads and saves the resource.
    /// </summary>
    /// <typeparam name="TDataSource">The type of the data source.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Registers <typeparamref name="TDataSource" /> as a scoped service if it isn't
    ///     registered already.
    /// </remarks>
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
    ///     Sets the property that identifies a resource.
    /// </summary>
    /// <typeparam name="TKey">The type of the key property.</typeparam>
    /// <param name="key">An expression that selects the key property.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The edit page, the view page and deletion need a key. An expression that doesn't select
    ///     a property of the resource with a public getter is a configuration problem, and the key
    ///     is left unchanged.
    /// </remarks>
    public ResourceBuilder<TResource> UseKey<TKey>(Expression<Func<TResource, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (
            key.Body is not MemberExpression { Member: PropertyInfo property } member
            || member.Expression != key.Parameters[0]
            || property.GetMethod?.IsPublic != true
        )
        {
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Report(
                    ConfigurationProblemKind.InvalidSelector,
                    ConfigurationProblemEffect.Ignored,
                    $"UseKey selects '{key}', which isn't a direct property with a public getter.",
                    "The key is left unchanged."
                )
            );

            return this;
        }

        Services.Configure<ResourceOptions<TResource>>(options => options.UseKey(property));

        return this;
    }
}
