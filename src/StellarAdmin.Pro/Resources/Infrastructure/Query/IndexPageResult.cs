using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.Resources.Infrastructure.Query;

/// <summary>
///     A page of entities, together with the filter and sort that produced it.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="PageNo">
///     The page that was returned — the requested page, clamped to the last page when
///     the request pointed past it.
/// </param>
/// <param name="PageSize">
///     The page size that was applied — the requested size, or the default when none
///     was requested.
/// </param>
/// <param name="Selection">
///     The validated values the user explicitly selected, for round-tripping in links.
/// </param>
public sealed record IndexPageResult<TEntity>(
    List<TEntity> Items,
    long Total,
    int PageNo,
    int PageSize,
    string? ActiveScopeSlug,
    string? ActiveSortField,
    DataGridSortDirection SortDirection,
    string? Search,
    IndexPageSelection Selection
)
    where TEntity : class;
