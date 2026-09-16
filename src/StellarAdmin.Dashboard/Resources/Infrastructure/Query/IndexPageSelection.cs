namespace StellarAdmin.Dashboard.Resources.Infrastructure.Query;

/// <summary>
///     The index page parameters the user explicitly selected, validated against the
///     page's configuration. A <c>null</c> member was not selected (or did not resolve)
///     and must be omitted from generated links, so URLs only carry values the user
///     chose.
/// </summary>
/// <param name="Scope">The canonical slug of the selected scope.</param>
/// <param name="SortBy">The canonical sort field of the selected sort.</param>
/// <param name="SortDirection">
///     The selected sort direction as its query string value ("asc" or "desc"), or
///     <c>null</c> when no sort is selected or the direction was implicit.
/// </param>
/// <param name="PageSize">The selected page size.</param>
public sealed record IndexPageSelection(
    string? Scope,
    string? SortBy,
    string? SortDirection,
    int? PageSize
);
