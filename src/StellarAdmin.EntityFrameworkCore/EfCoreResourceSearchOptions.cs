using System.Linq.Expressions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Stores how the index of a resource that EF Core loads is searched.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceSearchOptions<TEntity> : ResourceSearchOptions
    where TEntity : class
{
    /// <summary>
    ///     Gets or sets the delegate that returns the filter expression for a search term.
    /// </summary>
    public required Func<string, Expression<Func<TEntity, bool>>> Predicate { get; init; }
}
