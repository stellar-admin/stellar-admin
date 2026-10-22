using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures how the records of a resource that EF Core loads look, and which may be picked,
///     when another record refers to them, such as in a lookup.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceReferenceBuilder<TEntity>
    : ResourceReferenceBuilderBase<TEntity, EfCoreResourceReferenceBuilder<TEntity>>
    where TEntity : class
{
    internal EfCoreResourceReferenceBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Orders the records by an entity property.
    /// </summary>
    /// <typeparam name="TSort">The type of the property to order by.</typeparam>
    /// <param name="selector">An expression that selects the property to order by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="selector" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     By default, the records are in the order of the index's default sort, or else ordered by
    ///     their title.
    /// </remarks>
    public EfCoreResourceReferenceBuilder<TEntity> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        Services.Configure<EfCoreResourceReferenceOptions<TEntity>>(options =>
            options.OrderQuery = query => query.OrderBy(selector)
        );

        return this;
    }

    /// <summary>
    ///     Sets the properties a search matches.
    /// </summary>
    /// <param name="selector">An expression that selects a property to search.</param>
    /// <param name="moreSelectors">Expressions that select more properties to search.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="selector" /> or <paramref name="moreSelectors" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="moreSelectors" /> contains <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     A search matches records that contain the term in any of the properties, ignoring case.
    ///     By default, a search matches as the index's search does, or else the record's title.
    /// </remarks>
    public EfCoreResourceReferenceBuilder<TEntity> SearchOn(
        Expression<Func<TEntity, string?>> selector,
        params Expression<Func<TEntity, string?>>[] moreSelectors
    )
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(moreSelectors);
        if (moreSelectors.Any(item => item is null))
        {
            throw new ArgumentException(
                "The search selectors cannot contain null.",
                nameof(moreSelectors)
            );
        }

        Services.Configure<EfCoreResourceReferenceOptions<TEntity>>(options =>
            options.SearchExpressions = [selector, .. moreSelectors]
        );

        return this;
    }

    /// <summary>
    ///     Limits the records that may be picked.
    /// </summary>
    /// <param name="predicate">An expression that returns whether a record may be picked.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="predicate" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The filter applies to every field that refers to the resource, in searches and choices,
    ///     not to the selected items. Calling this more than once keeps the records that match
    ///     every filter.
    /// </remarks>
    public EfCoreResourceReferenceBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        Services.Configure<EfCoreResourceReferenceOptions<TEntity>>(options =>
            options.Filters.Add(predicate)
        );

        return this;
    }
}
