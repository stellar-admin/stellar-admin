using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

// Choices that load from an entity set for each request
internal sealed class EfCoreChoiceItems<TEntity, TValue>(
    EfCoreChoiceItemsOptions<TEntity, TValue> itemOptions
) : ChoiceItems
    where TEntity : class
{
    private readonly Type? _contextType = itemOptions.ContextType;
    private readonly Func<object?, Expression<Func<TEntity, bool>>>[] _formFilters =
        itemOptions.FormFilters.ToArray();
    private readonly EfCoreItemMedia<TEntity>? _media = itemOptions.Media;
    private readonly Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? _orderQuery =
        itemOptions.OrderQuery;
    private readonly Expression<Func<TEntity, ChoiceItemProjection>> _projection =
        CreateProjection(itemOptions);

    public override Type ValueType => typeof(TValue);

    public override async Task<IReadOnlyList<ChoiceItem>> LoadAsync(
        object model,
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        var db = EfCoreContexts.For(services).Get(services, typeof(TEntity), _contextType);
        IQueryable<TEntity> query = db.Set<TEntity>().AsNoTracking();
        foreach (var formFilter in _formFilters)
        {
            query = query.Where(formFilter(model));
        }

        if (_orderQuery is not null)
        {
            query = _orderQuery(query);
        }

        var rows = await query.Select(_projection).ToListAsync(cancellationToken);
        var items = new List<ChoiceItem>(rows.Count);

        foreach (var row in rows)
        {
            // Posted values bind in the current culture, as the editors match them
            var itemValue =
                Convert.ToString(row.Value, CultureInfo.CurrentCulture)
                ?? throw new InvalidOperationException("Choice item values cannot be null.");
            items.Add(
                new ChoiceItem(itemValue, row.Title)
                {
                    Description = row.Description,
                    Group = row.Group is null ? null : new ChoiceGroup(row.Group),
                    Media = _media?.Create(row.Media),
                }
            );
        }

        return items;
    }

    private static Expression<Func<TEntity, ChoiceItemProjection>> CreateProjection(
        EfCoreChoiceItemsOptions<TEntity, TValue> options
    )
    {
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        var constructor = typeof(ChoiceItemProjection).GetConstructor([
            typeof(TValue),
            typeof(string),
            typeof(string),
            typeof(string),
            typeof(string),
        ])!;

        return Expression.Lambda<Func<TEntity, ChoiceItemProjection>>(
            Expression.New(
                constructor,
                Rebind(options.ValueExpression, entity),
                Rebind(options.TitleExpression, entity),
                RebindOrNull(options.DescriptionExpression, entity),
                RebindOrNull(options.GroupExpression, entity),
                RebindOrNull(options.Media?.Selector, entity)
            ),
            entity
        );
    }

    private static Expression Rebind(LambdaExpression selector, ParameterExpression entity) =>
        new ReplaceParameterVisitor(selector.Parameters[0], entity).Visit(selector.Body)!;

    private static Expression RebindOrNull(
        LambdaExpression? selector,
        ParameterExpression entity
    ) => selector is null ? Expression.Constant(null, typeof(string)) : Rebind(selector, entity);

    private sealed record ChoiceItemProjection(
        TValue Value,
        string Title,
        string? Description,
        string? Group,
        string? Media
    );
}
