namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures how a lookup editor displays its field in the form.
/// </summary>
public sealed class LookupFieldOptions
{
    /// <summary>
    ///     Whether the selection can be cleared, or null to allow it when the field is optional.
    /// </summary>
    public bool? AllowClear { get; set; }

    /// <summary>
    ///     Hint text displayed while nothing is selected.
    /// </summary>
    public string? EmptyText { get; set; }

    /// <summary>
    ///     How the selected item is laid out, or null to use a card when the items have a description.
    /// </summary>
    public LookupSheetEditorLayout? Layout { get; set; }

    /// <summary>
    ///     Whether the selected item's media is displayed.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see langword="true" />.
    /// </remarks>
    public bool ShowMedia { get; set; } = true;
}
