using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the columns displayed on a resource's index page.
/// </summary>
public sealed class ResourceColumnsBuilder<TResource>
{
    private readonly IServiceCollection _services;

    internal ResourceColumnsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Adds a column.
    /// </summary>
    public ResourceColumnBuilder<TResource> Add<TProperty>(
        Expression<Func<TResource, TProperty>> field
    )
    {
        ArgumentNullException.ThrowIfNull(field);

        var column = new ResourceColumnBuilder<TResource>(field);
        _services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.Columns.Add(column.Build())
        );

        return column;
    }

    /// <summary>
    ///     Adds and configures a column.
    /// </summary>
    public ResourceColumnsBuilder<TResource> Add<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<ResourceColumnBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Add(field));

        return this;
    }

    /// <summary>
    ///     Removes all configured columns.
    /// </summary>
    public ResourceColumnsBuilder<TResource> Clear()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Index.Columns.Clear());

        return this;
    }
}
