using System.Linq.Expressions;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Infrastructure.Query;

internal static class QueryableSortExtensions
{
    extension<TEntity>(IQueryable<TEntity> query)
    {
        // Queryable.OrderBy needs the sorted property's type as a type argument, which a
        // configured field expression only carries at run time. The call is therefore
        // composed by hand and handed back to the provider.
        public IQueryable<TEntity> OrderByField(
            LambdaExpression fieldExpression,
            DataGridSortDirection direction
        )
        {
            var call = Expression.Call(
                typeof(Queryable),
                direction == DataGridSortDirection.Descending ? "OrderByDescending" : "OrderBy",
                [typeof(TEntity), fieldExpression.ReturnType],
                query.Expression,
                Expression.Quote(fieldExpression)
            );

            return query.Provider.CreateQuery<TEntity>(call);
        }
    }
}
