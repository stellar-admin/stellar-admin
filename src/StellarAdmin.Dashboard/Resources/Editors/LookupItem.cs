namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The selected item of a lookup editor.
/// </summary>
/// <param name="Text">The text displayed in the field.</param>
/// <param name="Description">The secondary text, or null.</param>
public sealed record LookupItem(string Text, string? Description);
