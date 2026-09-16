using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The sort applied to an index page, including a configured default sort.
/// </summary>
/// <param name="By">The field the list is sorted by, or <c>null</c> when the list is unsorted.</param>
/// <param name="Direction">
///     The direction the list is sorted in. Meaningful only when <see cref="By" /> is set.
/// </param>
public sealed record ResourceIndexSortViewModel(string? By, DataGridSortDirection Direction);
