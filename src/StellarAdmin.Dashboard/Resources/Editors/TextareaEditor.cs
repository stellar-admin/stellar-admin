namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a multi-line text area editor.
/// </summary>
public sealed class TextareaEditor : FieldEditor, IFieldEditor<TextareaEditorHandler>
{
    /// <summary>
    ///     Hint text displayed while the text area is empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    ///     The number of visible text lines, or null to grow with the content. A text area with a row count keeps that
    ///     height and scrolls longer text.
    /// </summary>
    public int? Rows { get; set; }
}
