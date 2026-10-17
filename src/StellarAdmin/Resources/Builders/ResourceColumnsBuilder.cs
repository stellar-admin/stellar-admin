using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the columns of a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceColumnsBuilder<TResource>
{
    private readonly IServiceCollection _services;

    internal ResourceColumnsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Adds a column.
    /// </summary>
    /// <typeparam name="TProperty">The type of the column's property.</typeparam>
    /// <param name="field">An expression that selects the property the column shows.</param>
    /// <returns>A builder that configures the column.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
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
    /// <typeparam name="TProperty">The type of the column's property.</typeparam>
    /// <param name="field">An expression that selects the property the column shows.</param>
    /// <param name="configure">A delegate that configures the column.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
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
    ///     Removes all columns added so far.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceColumnsBuilder<TResource> Clear()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Index.Columns.Clear());

        return this;
    }
}
