using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures choices loaded from an EF Core entity set.
/// </summary>
public sealed class EfCoreSelectListItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreSelectListItemsOptions<TEntity, TValue> _options;

    internal EfCoreSelectListItemsBuilder(EfCoreSelectListItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Adds an empty choice with the specified label before the entity choices.
    /// </summary>
    public EfCoreSelectListItemsBuilder<TEntity, TValue> IncludeEmptyOption(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        _options.EmptyOptionText = text;

        return this;
    }

    /// <summary>
    ///     Orders the choices by an entity property.
    /// </summary>
    public EfCoreSelectListItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }
}
