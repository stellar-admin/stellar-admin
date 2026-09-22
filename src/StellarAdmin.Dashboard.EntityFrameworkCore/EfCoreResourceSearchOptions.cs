using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures EF Core resource searching.
/// </summary>
public sealed class EfCoreResourceSearchOptions<TEntity> : ResourceSearchOptions
    where TEntity : class
{
    /// <summary>
    ///     The predicate factory for a search term.
    /// </summary>
    public required Func<string, Expression<Func<TEntity, bool>>> Predicate { get; init; }
}
