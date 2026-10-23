using System.Linq.Expressions;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

// The settings of lookup items that load from an entity set. The builder sets them without knowing the value's type,
// which the typed options below take from the value expression
internal abstract class EfCoreLookupItemsOptions<TEntity>
    where TEntity : class
{
    // False when EF Core can't translate the title, so searches and ordering don't use it
    internal bool CanQueryTitle { get; init; } = true;

    // The context UseDbContext pins, or a referenced resource's; otherwise, the one that maps the entity is found
    internal Type? ContextType { get; set; }

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

    private protected EfCoreLookupItemsOptions(Expression<Func<TEntity, string>> titleExpression)
    {
        TitleExpression = titleExpression;
    }

    internal static EfCoreLookupItemsOptions<TEntity> Create(
        Expression<Func<TEntity, object?>> value,
        Expression<Func<TEntity, string>> title
    )
    {
        var typedValue = EfCoreItemValue.Unbox(value);

        return (EfCoreLookupItemsOptions<TEntity>)
            Activator.CreateInstance(
                typeof(EfCoreLookupItemsOptions<,>).MakeGenericType(
                    typeof(TEntity),
                    typedValue.ReturnType
                ),
                typedValue,
                title
            )!;
    }

    internal abstract LookupItems CreateItems();
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
