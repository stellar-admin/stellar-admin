using System.Linq.Expressions;

namespace StellarAdmin.EntityFrameworkCore;

internal sealed class EfCoreLookupItemsOptions<TEntity, TValue>
    where TEntity : class
{
    // False when EF Core can't translate the title, so searches and ordering don't use it
    internal bool CanQueryTitle { get; init; } = true;

    internal Expression<Func<TEntity, string?>>? DescriptionExpression { get; set; }

    internal List<Func<object?, Expression<Func<TEntity, bool>>>> FormFilters { get; } = [];

    internal EfCoreItemMedia<TEntity>? Media { get; set; }

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; set; }

    internal (Type ModelType, string Navigation)? Reference { get; set; }

    internal IReadOnlyList<Expression<Func<TEntity, string?>>> SearchExpressions { get; set; } = [];

    internal Func<string, Expression<Func<TEntity, bool>>>? SearchPredicate { get; init; }

    internal Expression<Func<TEntity, string>> TitleExpression { get; }

    // Whether an item without a title is named by its value
    internal bool TitlesFallBackToValue { get; init; }

    internal Expression<Func<TEntity, TValue>> ValueExpression { get; }

    internal EfCoreLookupItemsOptions(
        Expression<Func<TEntity, TValue>> valueExpression,
        Expression<Func<TEntity, string>> titleExpression
    )
    {
        ValueExpression = valueExpression;
        TitleExpression = titleExpression;
    }
}
