using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Stores a scope that filters a resource's index with a predicate.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceScopeOptions<TEntity> : ResourceScopeOptions
    where TEntity : class
{
    /// <summary>
    ///     Gets the predicate that filters the resources.
    /// </summary>
    /// <value>
    ///     The predicate, or <see langword="null" /> to show every resource. The default is
    ///     <see langword="null" />.
    /// </value>
    public Expression<Func<TEntity, bool>>? Predicate { get; init; }
}
