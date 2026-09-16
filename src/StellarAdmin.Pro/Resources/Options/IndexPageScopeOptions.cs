using System.Linq.Expressions;

namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     A configured scope of an index page — a named slice of the list, rendered as a tab
///     above the data grid.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class IndexPageScopeOptions<TEntity>
    where TEntity : class
{
    /// <summary>Whether this scope is applied when the request does not specify one.</summary>
    public bool IsDefault { get; internal set; }

    /// <summary>
    ///     The predicate the scope filters the list by, or <c>null</c> for an unfiltered scope.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Predicate { get; }

    /// <summary>The value that identifies the scope in the query string.</summary>
    /// <remarks>Defaults to a slug made from <see cref="Title" />. Matched case-insensitively.</remarks>
    public string Slug { get; internal set; }

    /// <summary>The scope's tab label.</summary>
    public string Title { get; }

    internal IndexPageScopeOptions(
        string title,
        string slug,
        Expression<Func<TEntity, bool>>? predicate
    )
    {
        Title = title;
        Slug = slug;
        Predicate = predicate;
    }
}
