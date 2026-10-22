using System.Linq.Expressions;

namespace StellarAdmin.EntityFrameworkCore;

// How an EF Core resource's records are searched, ordered and filtered when another record refers to them
internal sealed class EfCoreResourceReferenceOptions<TEntity>
    where TEntity : class
{
    internal List<Expression<Func<TEntity, bool>>> Filters { get; } = [];

    internal Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderQuery { get; set; }

    internal IReadOnlyList<Expression<Func<TEntity, string?>>> SearchExpressions { get; set; } = [];
}
