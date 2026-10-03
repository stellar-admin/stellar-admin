namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     An item listed in a lookup editor's search results.
/// </summary>
/// <param name="Value">The value posted when the item is selected, formatted in the current culture.</param>
/// <param name="Title">The item's title.</param>
/// <param name="Description">The secondary text displayed below the item's title, or null.</param>
/// <param name="Media">The media displayed beside the title, or null.</param>
public sealed record LookupResult(
    string Value,
    string Title,
    string? Description,
    LookupMedia? Media
);
