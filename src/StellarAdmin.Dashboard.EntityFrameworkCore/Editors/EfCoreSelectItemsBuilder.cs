using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures choices loaded from an EF Core entity set.
/// </summary>
public sealed class EfCoreSelectItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreChoiceItemsOptions<TEntity, TValue> _options;

    internal EfCoreSelectItemsBuilder(EfCoreChoiceItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Adds an empty choice with the specified label before the entity choices.
    /// </summary>
    public EfCoreSelectItemsBuilder<TEntity, TValue> IncludeEmptyOption(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        _options.EmptyOptionText = text;

        return this;
    }

    /// <summary>
    ///     Orders the choices by an entity property.
    /// </summary>
    public EfCoreSelectItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }
}
