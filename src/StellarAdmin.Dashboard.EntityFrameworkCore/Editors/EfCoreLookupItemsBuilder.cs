using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures lookup items loaded from an EF Core entity set.
/// </summary>
public sealed class EfCoreLookupItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreLookupItemsOptions<TEntity, TValue> _options;

    internal EfCoreLookupItemsBuilder(EfCoreLookupItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Displays secondary text below each item's text.
    /// </summary>
    public EfCoreLookupItemsBuilder<TEntity, TValue> DescribeWith(
        Expression<Func<TEntity, string?>> description
    )
    {
        ArgumentNullException.ThrowIfNull(description);

        _options.DescriptionExpression = description;

        return this;
    }

    /// <summary>
    ///     Orders the items by an entity property. Items are ordered by their text by default.
    /// </summary>
    public EfCoreLookupItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }

    /// <summary>
    ///     Loads the selected item through a navigation of the form's entity. By default, the navigation is
    ///     inferred from the field's foreign key.
    /// </summary>
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
    ///     Searches the specified properties, ignoring case. The item text is searched by default.
    /// </summary>
    public EfCoreLookupItemsBuilder<TEntity, TValue> SearchOn(
        params Expression<Func<TEntity, string?>>[] selectors
    )
    {
        ArgumentNullException.ThrowIfNull(selectors);
        if (selectors.Length == 0 || selectors.Any(selector => selector is null))
        {
            throw new ArgumentException(
                "At least one search selector is required, and none can be null.",
                nameof(selectors)
            );
        }

        _options.SearchExpressions = selectors.ToArray();

        return this;
    }
}
