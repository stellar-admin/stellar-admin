namespace StellarAdmin.UI;

/// <summary>
///     Holds additional CSS classes for the parts of a rich text editor.
/// </summary>
public class RichTextEditorClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for the editor's container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the area the text is edited in.
    /// </summary>
    public string? Editor { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the toolbar.
    /// </summary>
    public string? Toolbar { get; set; }
}
