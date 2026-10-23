using System.Linq.Expressions;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

// The settings of choices that load from an entity set. The builder sets them without knowing the value's type, which
// the typed options below take from the value expression
internal abstract class EfCoreChoiceItemsOptions<TEntity>
    where TEntity : class
{
    // The context UseDbContext pins; otherwise, the one that maps the entity is found
    internal Type? ContextType { get; set; }

    internal Expression<Func<TEntity, string?>>? DescriptionExpression { get; set; }

    internal List<Func<object?, Expression<Func<TEntity, bool>>>> FormFilters { get; } = [];

    internal Expression<Func<TEntity, string?>>? GroupExpression { get; set; }

    internal EfCoreItemMedia<TEntity>? Media { get; set; }

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; set; }

    internal Expression<Func<TEntity, string>> TitleExpression { get; }

    private protected EfCoreChoiceItemsOptions(Expression<Func<TEntity, string>> titleExpression)
    {
        TitleExpression = titleExpression;
    }

    internal static EfCoreChoiceItemsOptions<TEntity> Create(
        Expression<Func<TEntity, object?>> value,
        Expression<Func<TEntity, string>> title
    )
    {
        var typedValue = EfCoreItemValue.Unbox(value);

        return (EfCoreChoiceItemsOptions<TEntity>)
            Activator.CreateInstance(
                typeof(EfCoreChoiceItemsOptions<,>).MakeGenericType(
                    typeof(TEntity),
                    typedValue.ReturnType
                ),
                typedValue,
                title
            )!;
    }

    internal abstract ChoiceItems CreateItems();
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
