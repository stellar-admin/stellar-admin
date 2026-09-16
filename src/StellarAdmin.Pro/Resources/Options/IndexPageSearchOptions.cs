namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The configured search options for an index page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class IndexPageSearchOptions<TEntity>
    where TEntity : class
{
    /// <summary>The search box placeholder to display.</summary>
    public string EffectivePlaceholder => Placeholder ?? "Search...";

    /// <summary>The search box placeholder text.</summary>
    /// <remarks>Defaults to "Search...".</remarks>
    public string? Placeholder { get; internal set; }

    /// <summary>Applies the search term to the query.</summary>
    public Func<IQueryable<TEntity>, string, IQueryable<TEntity>> Query { get; }

    internal IndexPageSearchOptions(Func<IQueryable<TEntity>, string, IQueryable<TEntity>> query)
    {
        Query = query;
    }
}
