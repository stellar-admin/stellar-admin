namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Represents a page of a lookup editor's search results.
/// </summary>
/// <param name="Items">
///     The items on the page, with each value formatted in the current culture.
/// </param>
/// <param name="HasMore">
///     <see langword="true" /> if more items follow the page; otherwise, <see langword="false" />.
/// </param>
/// <remarks>
///     Lookup editors ignore <see cref="ChoiceItem.Disabled" /> and
///     <see cref="ChoiceItem.Group" />.
/// </remarks>
public sealed record LookupResults(IReadOnlyList<ChoiceItem> Items, bool HasMore);
