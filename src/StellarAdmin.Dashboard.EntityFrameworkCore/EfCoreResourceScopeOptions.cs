using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     A named resource filter with an EF Core predicate.
/// </summary>
public sealed record EfCoreResourceScopeOptions<TEntity>(string Id, string Title)
    : ResourceScopeOptions(Id, Title)
    where TEntity : class
{
    /// <summary>
    ///     The filter predicate, or null for an unfiltered scope.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Predicate { get; init; }
}
