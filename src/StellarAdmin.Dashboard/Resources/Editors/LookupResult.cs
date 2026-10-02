namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     An item listed in a lookup editor's search results.
/// </summary>
/// <param name="Value">The value posted when the item is selected, formatted in the current culture.</param>
/// <param name="Text">The item's text.</param>
/// <param name="Description">The secondary text displayed below the item's text, or null.</param>
public sealed record LookupResult(string Value, string Text, string? Description);
