namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     A page of a lookup editor's search results.
/// </summary>
/// <param name="Items">The items on the page.</param>
/// <param name="HasMore">Whether more items follow the page.</param>
public sealed record LookupResults(IReadOnlyList<LookupResult> Items, bool HasMore);
