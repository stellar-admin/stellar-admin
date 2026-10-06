namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The selected item of a lookup editor.
/// </summary>
/// <param name="Title">The item's title.</param>
/// <param name="Description">The secondary text, or null.</param>
/// <param name="Media">The media displayed beside the title, or null.</param>
public sealed record LookupItem(string Title, string? Description, LookupMedia? Media);
