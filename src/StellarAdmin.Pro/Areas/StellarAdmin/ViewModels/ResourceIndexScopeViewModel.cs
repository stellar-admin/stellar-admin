namespace StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;

/// <summary>
///     A scope tab of an index page.
/// </summary>
/// <param name="Slug">The value identifying the scope in the index page's query string.</param>
/// <param name="Title">The scope's tab label.</param>
/// <param name="IsActive">Whether the scope is the one currently applied.</param>
/// <param name="QueryValue">
///     The value the tab's link carries in the scope parameter — the slug, or
///     <c>null</c> for the default scope so its link stays parameter-free.
/// </param>
public sealed record ResourceIndexScopeViewModel(
    string Slug,
    string Title,
    bool IsActive,
    string? QueryValue
);
