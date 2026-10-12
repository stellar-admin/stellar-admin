using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal static class EfCoreRowDetailLoader
{
    // Projects the entity with the resource ID; null when the ID is not a valid key or the entity does not exist
    public static Func<IServiceProvider, string, CancellationToken, Task<object?>> Create<
        TContext,
        TEntity,
        TModel
    >(Expression<Func<TEntity, TModel>> select)
        where TContext : DbContext
        where TEntity : class =>
        async (services, id, cancellationToken) =>
        {
            var db = services.GetRequiredService<TContext>();
            if (!EfCoreResourceKey.TryCreatePredicate<TEntity>(db, id, out var predicate))
            {
                return null;
            }

            // A list distinguishes a missing entity from a projection to a default value
            var models = await db.Set<TEntity>()
                .AsNoTracking()
                .Where(predicate)
                .Select(select)
                .Take(1)
                .ToListAsync(cancellationToken);

            return models.Count == 0 ? null : models[0];
        };
}
