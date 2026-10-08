namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     A page of a lookup editor's search results.
/// </summary>
/// <param name="Items">
///     The items on the page. Each value is formatted in the current culture; disabled states and groups are ignored.
/// </param>
/// <param name="HasMore">Whether more items follow the page.</param>
public sealed record LookupResults(IReadOnlyList<ChoiceItem> Items, bool HasMore);
