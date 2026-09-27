using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures choices loaded from an EF Core entity set for a checkbox group.
/// </summary>
public sealed class EfCoreCheckboxGroupItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreSelectListItemsOptions<TEntity, TValue> _options;

    internal EfCoreCheckboxGroupItemsBuilder(EfCoreSelectListItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Orders the choices by an entity property.
    /// </summary>
    public EfCoreCheckboxGroupItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }
}
