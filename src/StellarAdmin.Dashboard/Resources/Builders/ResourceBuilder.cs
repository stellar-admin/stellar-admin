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
    ///     Configures the create page.
    /// </summary>
    public ResourceBuilder<TResource> Create(Action<ResourceCreateBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures resource deletion.
    /// </summary>
    public ResourceBuilder<TResource> Delete(Action<ResourceDeleteBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Configures the edit page.
    /// </summary>
    public ResourceBuilder<TResource> Edit(Action<ResourceEditBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

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
        _services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.KeySelector = resource =>
                Convert.ToString(selector(resource), CultureInfo.InvariantCulture)!;
            options.KeyPropertyName = property.Name;
        });

        return this;
    }
}
