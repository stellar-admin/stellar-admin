using System.Linq.Expressions;

namespace StellarAdmin.EntityFrameworkCore;

// The settings that lookup items and choices loading from an entity set share. The builders set them without knowing
// the value's type, which the typed options take from the value expression
internal abstract class EfCoreItemsOptions<TEntity, TItems>
    where TEntity : class
{
    // The context UseDbContext pins, or a referenced resource's; otherwise, the one that maps the entity is found
    internal Type? ContextType { get; set; }

    internal Expression<Func<TEntity, string?>>? DescriptionExpression { get; set; }

    internal List<Func<object?, Expression<Func<TEntity, bool>>>> FormFilters { get; } = [];

    internal EfCoreItemMedia<TEntity>? Media { get; set; }

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; set; }

    internal Expression<Func<TEntity, string>> TitleExpression { get; }

    private protected EfCoreItemsOptions(Expression<Func<TEntity, string>> titleExpression)
    {
        TitleExpression = titleExpression;
    }

    internal abstract TItems CreateItems();

    // Creates the typed options of a definition, such as EfCoreLookupItemsOptions<,>, for the type the value
    // expression boxes
    private protected static TOptions CreateTyped<TOptions>(
        Type definition,
        Expression<Func<TEntity, object?>> value,
        Expression<Func<TEntity, string>> title
    )
        where TOptions : EfCoreItemsOptions<TEntity, TItems>
    {
        var typedValue = EfCoreItemValue.Unbox(value);

        return (TOptions)
            Activator.CreateInstance(
                definition.MakeGenericType(typeof(TEntity), typedValue.ReturnType),
                typedValue,
                title
            )!;
    }
}
