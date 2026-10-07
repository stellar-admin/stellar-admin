using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal sealed class EfCoreChoiceItemsOptions<TEntity, TValue>
    where TEntity : class
{
    internal Expression<Func<TEntity, string?>>? DescriptionExpression { get; set; }

    internal string? EmptyOptionText { get; set; }

    internal Expression<Func<TEntity, string?>>? GroupExpression { get; set; }

    internal Expression<Func<TEntity, string>> TextExpression { get; }

    internal Expression<Func<TEntity, TValue>> ValueExpression { get; }

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; set; }

    internal EfCoreChoiceItemsOptions(
        Expression<Func<TEntity, TValue>> valueExpression,
        Expression<Func<TEntity, string>> textExpression
    )
    {
        ValueExpression = valueExpression;
        TextExpression = textExpression;
    }
}
