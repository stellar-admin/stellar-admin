using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore;

// Projects the entity with the resource ID; null when the ID is not a valid key or the entity does not exist
internal sealed class EfCoreRowDetailLoader<TContext, TEntity, TModel>(
    Expression<Func<TEntity, TModel>> select
) : ResourceRowDetailLoader
    where TContext : DbContext
    where TEntity : class
{
    public override async Task<object?> LoadAsync(
        IServiceProvider services,
        string key,
        CancellationToken cancellationToken
    )
    {
        var db = services.GetRequiredService<TContext>();
        if (!EfCoreResourceKey.TryCreatePredicate<TEntity>(db, key, out var predicate))
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
    }
}
