using System.Linq.Expressions;

namespace StellarAdmin.EntityFrameworkCore;

internal sealed class EfCoreChoiceItemsOptions<TEntity, TValue>
    where TEntity : class
{
    // The context UseDbContext pins; otherwise, the one that maps the entity is found
    internal Type? ContextType { get; set; }

    internal Expression<Func<TEntity, string?>>? DescriptionExpression { get; set; }

    internal List<Func<object?, Expression<Func<TEntity, bool>>>> FormFilters { get; } = [];

    internal Expression<Func<TEntity, string?>>? GroupExpression { get; set; }

    internal EfCoreItemMedia<TEntity>? Media { get; set; }

    internal Expression<Func<TEntity, string>> TitleExpression { get; }

    internal Expression<Func<TEntity, TValue>> ValueExpression { get; }

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; set; }

    internal EfCoreChoiceItemsOptions(
        Expression<Func<TEntity, TValue>> valueExpression,
        Expression<Func<TEntity, string>> titleExpression
    )
    {
        ValueExpression = valueExpression;
        TitleExpression = titleExpression;
    }
}
