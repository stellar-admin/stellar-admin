using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal static class EfCoreChoiceItemsLoader
{
    internal static void Configure<TContext, TEntity, TValue>(
        ChoiceItemsEditorOptions options,
        EfCoreSelectListItemsOptions<TEntity, TValue> itemOptions
    )
        where TContext : DbContext
        where TEntity : class
    {
        var projection = CreateProjection(itemOptions.ValueExpression, itemOptions.TextExpression);
        var orderQuery = itemOptions.OrderQuery;
        var emptyOptionText = itemOptions.EmptyOptionText;

        options.UseItems(
            async (services, cancellationToken) =>
            {
                var db = services.GetRequiredService<TContext>();
                IQueryable<TEntity> query = db.Set<TEntity>().AsNoTracking();
                if (orderQuery is not null)
                {
                    query = orderQuery(query);
                }

                var rows = await query.Select(projection).ToListAsync(cancellationToken);
                var items = new List<SelectListItem>(
                    rows.Count + (emptyOptionText is null ? 0 : 1)
                );
                if (emptyOptionText is not null)
                {
                    items.Add(new SelectListItem(emptyOptionText, ""));
                }

                foreach (var row in rows)
                {
                    var itemValue =
                        Convert.ToString(row.Value, CultureInfo.InvariantCulture)
                        ?? throw new InvalidOperationException(
                            "Choice item values cannot be null."
                        );
                    items.Add(new SelectListItem(row.Text, itemValue));
                }

                return items;
            }
        );
    }

    private static Expression<Func<TEntity, ChoiceItemProjection<TValue>>> CreateProjection<
        TEntity,
        TValue
    >(Expression<Func<TEntity, TValue>> value, Expression<Func<TEntity, string>> text)
    {
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        var valueBody = new ReplaceParameterVisitor(value.Parameters[0], entity).Visit(value.Body)!;
        var textBody = new ReplaceParameterVisitor(text.Parameters[0], entity).Visit(text.Body)!;
        var constructor = typeof(ChoiceItemProjection<TValue>).GetConstructor([
            typeof(TValue),
            typeof(string),
        ])!;

        return Expression.Lambda<Func<TEntity, ChoiceItemProjection<TValue>>>(
            Expression.New(constructor, valueBody, textBody),
            entity
        );
    }

    private sealed record ChoiceItemProjection<TValue>(TValue Value, string Text);

    private sealed class ReplaceParameterVisitor(
        ParameterExpression source,
        ParameterExpression target
    ) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == source ? target : base.VisitParameter(node);
    }
}
