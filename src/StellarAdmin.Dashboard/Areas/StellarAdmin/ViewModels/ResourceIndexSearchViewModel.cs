namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The search box of an index page.
/// </summary>
/// <param name="Term">The search term entered, or <c>null</c> when the list is unfiltered.</param>
/// <param name="Placeholder">The search box placeholder.</param>
public sealed record ResourceIndexSearchViewModel(string? Term, string Placeholder);
