using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the lookup items that load from an entity set.
/// </summary>
/// <typeparam name="TEntity">The type of the entities.</typeparam>
/// <typeparam name="TValue">The type of the value that identifies an entity.</typeparam>
public sealed class EfCoreLookupItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreLookupItemsOptions<TEntity, TValue> _options;

    internal EfCoreLookupItemsBuilder(EfCoreLookupItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Orders the items by an entity property.
    /// </summary>
    /// <typeparam name="TSort">The type of the property to order by.</typeparam>
    /// <param name="selector">An expression that selects the property to order by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="selector" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     By default, the items are ordered by their title.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }

    /// <summary>
    ///     Sets the navigation of the form's entity that loads the selected item.
    /// </summary>
    /// <typeparam name="TModel">The type of the form's entity.</typeparam>
    /// <param name="navigation">An expression that selects the navigation property.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="navigation" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="navigation" /> doesn't select a property of
    ///     <typeparamref name="TModel" />.
    /// </exception>
    /// <remarks>
    ///     By default, the navigation is inferred from the field's foreign key.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> ReferenceFrom<TModel>(
        Expression<Func<TModel, TEntity?>> navigation
    )
    {
        ArgumentNullException.ThrowIfNull(navigation);

        if (
            navigation.Body is not MemberExpression { Member: var member, Expression: var target }
            || target != navigation.Parameters[0]
        )
        {
            throw new ArgumentException(
                "The navigation must select a property of the model.",
                nameof(navigation)
            );
        }

        _options.Reference = (typeof(TModel), member.Name);

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
    ///     A search matches items that contain the term in any of the properties, ignoring case.
    ///     By default, the search matches the item's title.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> SearchOn(
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

        _options.SearchExpressions = [selector, .. moreSelectors];

        return this;
    }

    /// <summary>
    ///     Shows an avatar beside each item's title.
    /// </summary>
    /// <param name="imageUrl">
    ///     An expression that selects the image URL, or <see langword="null" /> or an empty string
    ///     to show the initials of the item's title.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseAvatar(
        Expression<Func<TEntity, string?>> imageUrl
    )
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        _options.Media = EfCoreItemMedia<TEntity>.Avatar(imageUrl);

        return this;
    }

    /// <summary>
    ///     Shows a short code beside each item's title.
    /// </summary>
    /// <param name="code">
    ///     An expression that selects the code, or <see langword="null" /> or an empty string to
    ///     show no media for the item.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="code" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseCode(
        Expression<Func<TEntity, string?>> code
    )
    {
        ArgumentNullException.ThrowIfNull(code);

        _options.Media = EfCoreItemMedia<TEntity>.Code(code);

        return this;
    }

    /// <summary>
    ///     Loads the items with a database context.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     By default, the registered database context whose model maps the entity is used. Pick
    ///     one when several contexts map it.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseDbContext<TContext>()
        where TContext : DbContext
    {
        _options.ContextType = typeof(TContext);

        return this;
    }

    /// <summary>
    ///     Shows secondary text below each item's title.
    /// </summary>
    /// <param name="description">
    ///     An expression that selects the description, or <see langword="null" /> to show none.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="description" /> is <see langword="null" />.
    /// </exception>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseDescription(
        Expression<Func<TEntity, string?>> description
    )
    {
        ArgumentNullException.ThrowIfNull(description);

        _options.DescriptionExpression = description;

        return this;
    }

    /// <summary>
    ///     Shows a registered icon beside each item's title.
    /// </summary>
    /// <param name="iconName">
    ///     An expression that selects the icon name, or <see langword="null" /> or an empty string
    ///     to show no media for the item.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="iconName" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseIcon(
        Expression<Func<TEntity, string?>> iconName
    )
    {
        ArgumentNullException.ThrowIfNull(iconName);

        _options.Media = EfCoreItemMedia<TEntity>.Icon(iconName);

        return this;
    }

    /// <summary>
    ///     Shows a square image beside each item's title.
    /// </summary>
    /// <param name="imageUrl">
    ///     An expression that selects the image URL, or <see langword="null" /> or an empty string
    ///     to show no media for the item.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseImage(
        Expression<Func<TEntity, string?>> imageUrl
    )
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        _options.Media = EfCoreItemMedia<TEntity>.Image(imageUrl);

        return this;
    }

    /// <summary>
    ///     Filters the items by the form's values.
    /// </summary>
    /// <typeparam name="TModel">The type of the form's model.</typeparam>
    /// <param name="filter">
    ///     An expression that returns whether an entity is an item, from the entity and the form's
    ///     model.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="filter" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Declare each field the filter reads with <c>DependsOn</c>: the form holds only those
    ///     fields' values, and the key on the edit form. The filter applies to searches, not to
    ///     the selected items. Calling this more than once keeps the items that match every filter.
    /// </remarks>
    public EfCoreLookupItemsBuilder<TEntity, TValue> Where<TModel>(
        Expression<Func<TEntity, TModel, bool>> filter
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        _options.FormFilters.Add(EfCoreFormFilter.Create(filter));

        return this;
    }
}
