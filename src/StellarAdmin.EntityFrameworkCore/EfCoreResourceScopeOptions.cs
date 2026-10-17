using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Stores a scope that filters a resource's index with a predicate.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <param name="Id">The identifier of the scope.</param>
/// <param name="Title">The label of the scope's tab.</param>
public sealed record EfCoreResourceScopeOptions<TEntity>(string Id, string Title)
    : ResourceScopeOptions(Id, Title)
    where TEntity : class
{
    /// <summary>
    ///     Gets or sets the predicate that filters the resources.
    /// </summary>
    /// <value>
    ///     The predicate, or <see langword="null" /> to show every resource. The default is
    ///     <see langword="null" />.
    /// </value>
    public Expression<Func<TEntity, bool>>? Predicate { get; init; }
}
