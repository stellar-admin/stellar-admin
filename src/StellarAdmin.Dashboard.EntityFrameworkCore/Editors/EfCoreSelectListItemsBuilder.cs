using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures choices loaded from an EF Core entity set.
/// </summary>
public sealed class EfCoreSelectListItemsBuilder<TEntity>
{
    internal string? EmptyOptionText { get; private set; }

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; private set; }

    /// <summary>
    ///     Adds an empty choice with the specified label before the entity choices.
    /// </summary>
    public EfCoreSelectListItemsBuilder<TEntity> IncludeEmptyOption(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        EmptyOptionText = text;

        return this;
    }

    /// <summary>
    ///     Orders the choices by an entity property.
    /// </summary>
    public EfCoreSelectListItemsBuilder<TEntity> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        OrderQuery = query => query.OrderBy(selector);

        return this;
    }
}
