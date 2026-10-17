namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores additional CSS classes for the parts of a multi-select lookup editor.
/// </summary>
public sealed class MultiLookupSheetEditorClassNames : EditorClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for the media of the selected items and the search
    ///     results.
    /// </summary>
    /// <remarks>
    ///     A size utility replaces the default size.
    /// </remarks>
    public string? Media { get; set; }
}
