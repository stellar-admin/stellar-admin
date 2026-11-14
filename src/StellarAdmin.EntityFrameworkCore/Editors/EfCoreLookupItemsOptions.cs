using System.Linq.Expressions;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

// The settings of lookup items that load from an entity set
internal abstract class EfCoreLookupItemsOptions<TEntity>(
    Expression<Func<TEntity, string>> titleExpression
) : EfCoreItemsOptions<TEntity, LookupItems>(titleExpression)
    where TEntity : class
{
    // False when EF Core can't translate the title, so searches and ordering don't use it
    internal bool CanQueryTitle { get; init; } = true;

    internal (Type ModelType, string Navigation)? Reference { get; set; }

    internal IReadOnlyList<Expression<Func<TEntity, string?>>> SearchExpressions { get; set; } = [];

    internal Func<string, Expression<Func<TEntity, bool>>>? SearchPredicate { get; init; }

    // Whether an item without a title is named by its value
    internal bool TitlesFallBackToValue { get; init; }

    internal static EfCoreLookupItemsOptions<TEntity> Create(
        Expression<Func<TEntity, object?>> value,
        Expression<Func<TEntity, string>> title
    ) =>
        CreateTyped<EfCoreLookupItemsOptions<TEntity>>(
            typeof(EfCoreLookupItemsOptions<,>),
            value,
            title
        );
}

internal sealed class EfCoreLookupItemsOptions<TEntity, TValue>(
    Expression<Func<TEntity, TValue>> valueExpression,
    Expression<Func<TEntity, string>> titleExpression
) : EfCoreLookupItemsOptions<TEntity>(titleExpression)
    where TEntity : class
{
    internal Expression<Func<TEntity, TValue>> ValueExpression { get; } = valueExpression;

    internal override LookupItems CreateItems() => new EfCoreLookupItems<TEntity, TValue>(this);
}
