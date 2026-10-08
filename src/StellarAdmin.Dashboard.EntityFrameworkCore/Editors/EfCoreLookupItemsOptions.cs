using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal sealed class EfCoreLookupItemsOptions<TEntity, TValue>
    where TEntity : class
{
    internal Expression<Func<TEntity, string?>>? DescriptionExpression { get; set; }

    internal EfCoreItemMedia<TEntity>? Media { get; set; }

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; set; }

    internal (Type ModelType, string Navigation)? Reference { get; set; }

    internal IReadOnlyList<Expression<Func<TEntity, string?>>> SearchExpressions { get; set; } = [];

    internal Expression<Func<TEntity, string>> TitleExpression { get; }

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
