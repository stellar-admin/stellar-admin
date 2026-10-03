using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Editors;

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
    ///     Orders the items by an entity property. Items are ordered by their title by default.
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
    ///     Searches the specified properties, ignoring case. The item title is searched by default.
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

    /// <summary>
    ///     Displays an avatar beside each item's title, with the title's initials when the image URL is null.
    ///     Replaces a code.
    /// </summary>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseAvatar(
        Expression<Func<TEntity, string?>> imageUrl
    )
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        _options.MediaType = LookupMediaType.Avatar;
        _options.MediaExpression = imageUrl;

        return this;
    }

    /// <summary>
    ///     Displays a short code beside each item's title. Replaces an avatar.
    /// </summary>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseCode(
        Expression<Func<TEntity, string?>> code
    )
    {
        ArgumentNullException.ThrowIfNull(code);

        _options.MediaType = LookupMediaType.Code;
        _options.MediaExpression = code;

        return this;
    }

    /// <summary>
    ///     Displays secondary text below each item's title.
    /// </summary>
    public EfCoreLookupItemsBuilder<TEntity, TValue> UseDescription(
        Expression<Func<TEntity, string?>> description
    )
    {
        ArgumentNullException.ThrowIfNull(description);

        _options.DescriptionExpression = description;

        return this;
    }
}
