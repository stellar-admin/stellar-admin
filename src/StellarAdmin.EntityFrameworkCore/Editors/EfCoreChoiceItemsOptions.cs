using System.Linq.Expressions;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

// The settings of choices that load from an entity set
internal abstract class EfCoreChoiceItemsOptions<TEntity>(
    Expression<Func<TEntity, string>> titleExpression
) : EfCoreItemsOptions<TEntity, ChoiceItems>(titleExpression)
    where TEntity : class
{
    internal Expression<Func<TEntity, string?>>? GroupExpression { get; set; }

    internal static EfCoreChoiceItemsOptions<TEntity> Create(
        Expression<Func<TEntity, object?>> value,
        Expression<Func<TEntity, string>> title
    ) =>
        CreateTyped<EfCoreChoiceItemsOptions<TEntity>>(
            typeof(EfCoreChoiceItemsOptions<,>),
            value,
            title
        );
}

internal sealed class EfCoreChoiceItemsOptions<TEntity, TValue>(
    Expression<Func<TEntity, TValue>> valueExpression,
    Expression<Func<TEntity, string>> titleExpression
) : EfCoreChoiceItemsOptions<TEntity>(titleExpression)
    where TEntity : class
{
    internal Expression<Func<TEntity, TValue>> ValueExpression { get; } = valueExpression;

    internal override ChoiceItems CreateItems() => new EfCoreChoiceItems<TEntity, TValue>(this);
}
