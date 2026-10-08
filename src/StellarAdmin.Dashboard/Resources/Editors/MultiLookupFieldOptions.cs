namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures how a multi-select lookup editor displays its field in the form.
/// </summary>
public sealed class MultiLookupFieldOptions
{
    /// <summary>
    ///     Hint text displayed while nothing is selected.
    /// </summary>
    public string? EmptyText { get; set; }

    /// <summary>
    ///     How the selected items are laid out.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="MultiLookupSheetEditorLayout.Chips" />.
    /// </remarks>
    public MultiLookupSheetEditorLayout Layout { get; set; } = MultiLookupSheetEditorLayout.Chips;

    /// <summary>
    ///     Whether the selected items' media is displayed.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see langword="true" />.
    /// </remarks>
    public bool ShowMedia { get; set; } = true;
}
