namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores additional CSS classes for the parts of a slider editor.
/// </summary>
public sealed class SliderEditorClassNames : EditorClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for each mark.
    /// </summary>
    public string? Mark { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for each mark's label.
    /// </summary>
    public string? MarkLabel { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the container that holds the marks.
    /// </summary>
    public string? Marks { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the value shown beside the label.
    /// </summary>
    public string? Value { get; set; }
}
