using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

// The items of fields that refer to an EF Core resource: its entities, valued by their primary key and named by the
// resource's title. Searches match the reference's properties, then the index's search, then the title; the items
// are ordered by the reference's order, then the index's default sort, then the title.
internal static class EfCoreResourceReference
{
    private static readonly MethodInfo CreateFormFilterMethod = typeof(EfCoreFormFilter).GetMethod(
        nameof(EfCoreFormFilter.Create)
    )!;
    private static readonly MethodInfo CreateItemsMethod =
        typeof(EfCoreResourceReference).GetMethod(
            nameof(CreateItems),
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

    public static void Configure<TContext, TEntity>(
        ResourceOptions<TEntity> options,
        EfCoreResourceReferenceOptions<TEntity> reference,
        TContext db,
        PropertyInfo key
    )
        where TContext : DbContext
        where TEntity : class
    {
        // A title EF Core can't translate is still shown, but searches and ordering can't use it
        var title = options.Title!;
        var queriesTitle =
            reference.OrderQuery is null && FindDefaultOrder(options) is null
            || reference.SearchExpressions.Count == 0
                && options.Index.Search is not EfCoreResourceSearchOptions<TEntity>;
        var canQueryTitle = !queriesTitle || CanTranslate(db.Set<TEntity>().OrderBy(title));
        if (!canQueryTitle)
        {
            options.ReportProblem(
                EfCoreConfigurationEvents.UntranslatableTitle,
                $"Fields that refer to {options.PluralLabel} order their items by key and don't search them, because EF Core can't translate the `{typeof(TEntity).Name}` title to SQL.",
                "Select a title EF Core can translate with `UseTitle`, or set `SearchOn` and `OrderBy` in `Reference(...)`."
            );
        }

        var createItems = CreateItemsMethod.MakeGenericMethod(
            typeof(TContext),
            typeof(TEntity),
            key.PropertyType
        );
        options.UseReferenceItems(filters =>
            (ResourceReferenceItems)
                createItems.Invoke(null, [options, reference, key, canQueryTitle, filters])!
        );
    }

    private static bool CanTranslate(IQueryable query)
    {
        try
        {
            _ = query.ToQueryString();

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static ResourceReferenceItems CreateItems<TContext, TEntity, TKey>(
        ResourceOptions<TEntity> options,
        EfCoreResourceReferenceOptions<TEntity> reference,
        PropertyInfo key,
        bool canQueryTitle,
        IReadOnlyList<LambdaExpression> filters
    )
        where TContext : DbContext
        where TEntity : class
    {
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        var itemOptions = new EfCoreLookupItemsOptions<TEntity, TKey>(
            Expression.Lambda<Func<TEntity, TKey>>(Expression.Property(entity, key), entity),
            options.Title!
        )
        {
            CanQueryTitle = canQueryTitle,
            DescriptionExpression = options.Reference.Description,
            Media = CreateMedia(options.Reference),
            OrderQuery = reference.OrderQuery ?? FindDefaultOrder(options),
            SearchExpressions = reference.SearchExpressions,
            SearchPredicate =
                reference.SearchExpressions.Count == 0
                && options.Index.Search is EfCoreResourceSearchOptions<TEntity> search
                    ? search.Predicate
                    : null,
            TitlesFallBackToValue = true,
        };
        foreach (var filter in reference.Filters)
        {
            itemOptions.FormFilters.Add(_ => filter);
        }

        foreach (var filter in filters)
        {
            itemOptions.FormFilters.Add(
                (Func<object?, Expression<Func<TEntity, bool>>>)
                    CreateFormFilterMethod
                        .MakeGenericMethod(typeof(TEntity), filter.Parameters[1].Type)
                        .Invoke(null, [filter])!
            );
        }

        return new EfCoreLookupItems<TContext, TEntity, TKey>(itemOptions);
    }

    private static EfCoreItemMedia<TEntity>? CreateMedia<TEntity>(
        ResourceReferenceOptions<TEntity> reference
    ) =>
        reference.Media is not { } media ? null
        : reference.MediaType == typeof(ItemMedia.Avatar) ? EfCoreItemMedia<TEntity>.Avatar(media)
        : reference.MediaType == typeof(ItemMedia.Code) ? EfCoreItemMedia<TEntity>.Code(media)
        : reference.MediaType == typeof(ItemMedia.Icon) ? EfCoreItemMedia<TEntity>.Icon(media)
        : reference.MediaType == typeof(ItemMedia.Image) ? EfCoreItemMedia<TEntity>.Image(media)
        : null;

    // The order of the index's default sort, when its column is sortable
    private static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? FindDefaultOrder<TEntity>(
        ResourceOptions<TEntity> options
    )
    {
        if (
            options.Index.DefaultSort is not { } sort
            || options.Index.Columns.FirstOrDefault(column =>
                column.Sortable && column.SortField == sort.Field
            )
                is not { } column
        )
        {
            return null;
        }

        var selector = column.SortExpression ?? column.FieldExpression;
        var method =
            sort.Direction == ResourceSortDirection.Descending
                ? nameof(Queryable.OrderByDescending)
                : nameof(Queryable.OrderBy);

        return query =>
            (IOrderedQueryable<TEntity>)
                query.Provider.CreateQuery<TEntity>(
                    Expression.Call(
                        typeof(Queryable),
                        method,
                        [typeof(TEntity), selector.ReturnType],
                        query.Expression,
                        Expression.Quote(selector)
                    )
                );
    }
}
