using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal static class EfCoreChoiceItemsLoader
{
    internal static void Configure<TContext, TEntity, TValue>(
        ChoiceEditor editor,
        EfCoreChoiceItemsOptions<TEntity, TValue> itemOptions
    )
        where TContext : DbContext
        where TEntity : class
    {
        var projection = CreateProjection(itemOptions);
        var orderQuery = itemOptions.OrderQuery;
        var emptyOptionText = itemOptions.EmptyOptionText;

        editor.UseItems(
            async (services, cancellationToken) =>
            {
                var db = services.GetRequiredService<TContext>();
                IQueryable<TEntity> query = db.Set<TEntity>().AsNoTracking();
                if (orderQuery is not null)
                {
                    query = orderQuery(query);
                }

                var rows = await query.Select(projection).ToListAsync(cancellationToken);
                var items = new List<ChoiceItem>(rows.Count + (emptyOptionText is null ? 0 : 1));
                if (emptyOptionText is not null)
                {
                    items.Add(new ChoiceItem("", emptyOptionText));
                }

                foreach (var row in rows)
                {
                    // Posted values bind in the current culture, as the editors match them
                    var itemValue =
                        Convert.ToString(row.Value, CultureInfo.CurrentCulture)
                        ?? throw new InvalidOperationException(
                            "Choice item values cannot be null."
                        );
                    items.Add(
                        new ChoiceItem(itemValue, row.Text)
                        {
                            Description = row.Description,
                            Group = row.Group is null ? null : new ChoiceGroup(row.Group),
                        }
                    );
                }

                return items;
            }
        );
    }

    private static Expression<Func<TEntity, ChoiceItemProjection<TValue>>> CreateProjection<
        TEntity,
        TValue
    >(EfCoreChoiceItemsOptions<TEntity, TValue> options)
        where TEntity : class
    {
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        var constructor = typeof(ChoiceItemProjection<TValue>).GetConstructor([
            typeof(TValue),
            typeof(string),
            typeof(string),
            typeof(string),
        ])!;

        return Expression.Lambda<Func<TEntity, ChoiceItemProjection<TValue>>>(
            Expression.New(
                constructor,
                Rebind(options.ValueExpression, entity),
                Rebind(options.TextExpression, entity),
                RebindOrNull(options.DescriptionExpression, entity),
                RebindOrNull(options.GroupExpression, entity)
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

    private sealed record ChoiceItemProjection<TValue>(
        TValue Value,
        string Text,
        string? Description,
        string? Group
    );
}
