using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures EF Core choices for select list editors.
/// </summary>
public static class SelectListEditorOptionsExtensions
{
    extension(SelectListEditorOptions options)
    {
        /// <summary>
        ///     Loads select choices from an EF Core entity set using required value and text selectors.
        /// </summary>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> text,
            Action<EfCoreSelectListItemsBuilder<TEntity>>? configure = null
        )
            where TContext : DbContext
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(text);

            var builder = new EfCoreSelectListItemsBuilder<TEntity>();
            configure?.Invoke(builder);
            var projection = CreateProjection(value, text);
            var orderQuery = builder.OrderQuery;
            var emptyOptionText = builder.EmptyOptionText;

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
                                "Select list item values cannot be null."
                            );
                        items.Add(new SelectListItem(row.Text, itemValue));
                    }

                    return items;
                }
            );
        }
    }

    private static Expression<Func<TEntity, SelectListItemProjection<TValue>>> CreateProjection<
        TEntity,
        TValue
    >(Expression<Func<TEntity, TValue>> value, Expression<Func<TEntity, string>> text)
    {
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        var valueBody = new ReplaceParameterVisitor(value.Parameters[0], entity).Visit(value.Body)!;
        var textBody = new ReplaceParameterVisitor(text.Parameters[0], entity).Visit(text.Body)!;
        var constructor = typeof(SelectListItemProjection<TValue>).GetConstructor([
            typeof(TValue),
            typeof(string),
        ])!;

        return Expression.Lambda<Func<TEntity, SelectListItemProjection<TValue>>>(
            Expression.New(constructor, valueBody, textBody),
            entity
        );
    }

    private sealed record SelectListItemProjection<TValue>(TValue Value, string Text);

    private sealed class ReplaceParameterVisitor(
        ParameterExpression source,
        ParameterExpression target
    ) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == source ? target : base.VisitParameter(node);
    }
}
