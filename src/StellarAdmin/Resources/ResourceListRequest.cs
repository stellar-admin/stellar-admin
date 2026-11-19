using System.Linq.Expressions;

namespace StellarAdmin.Resources;

/// <summary>
///     Specifies which resources a data source returns and how they are sorted and paged.
/// </summary>
public sealed record ResourceListRequest
{
    /// <summary>
    ///     Gets the expressions the caller reads from each returned resource, such as an index
    ///     column's <c>p =&gt; p.Category.Name</c>.
    /// </summary>
    /// <value>
    ///     The expressions, which take the resource as their parameter. The default is empty.
    /// </value>
    /// <remarks>
    ///     A data source can use them to load related data the caller reads. A data source that
    ///     ignores them still works.
    /// </remarks>
    public IReadOnlyList<LambdaExpression> Fields { get; init; } = [];

    /// <summary>
    ///     Gets the keys of the only resources to return, or <see langword="null" /> to return all
    ///     matching resources.
    /// </summary>
    /// <remarks>
    ///     Keys are set to reload rows after an inline save, with no scope, search or paging. They
    ///     are formatted in the invariant culture. A data source that ignores them still works, but
    ///     returns more resources than needed.
    /// </remarks>
    public IReadOnlyList<string>? Keys { get; init; }

    /// <summary>
    ///     Gets the key of the index layout the list is for.
    /// </summary>
    /// <value>
    ///     The key, or <see langword="null" /> when the list isn't for the index page, such as the
    ///     items of a lookup. The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A data source can use the key when its layouts need different data, such as related
    ///     records that only one table shows.
    /// </remarks>
    public string? Layout { get; init; }

    /// <summary>
    ///     Gets the requested page, or <see langword="null" /> to return all matching resources.
    /// </summary>
    public ResourcePaging? Paging { get; init; }

    /// <summary>
    ///     Gets the identifier of the selected scope, or <see langword="null" /> when no scope is
    ///     selected.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    ///     Gets the search term, or <see langword="null" /> when the list isn't searched.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Gets the requested sort order, or <see langword="null" /> to use the data source's
    ///     default order.
    /// </summary>
    public ResourceSort? Sort { get; init; }
}
