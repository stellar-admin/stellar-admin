namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Additional CSS classes for a slider editor, its value and its marks.
/// </summary>
public class SliderEditorClassNames : EditorClassNames
{
    /// <summary>
    ///     Additional CSS classes applied to each mark.
    /// </summary>
    public string? Mark { get; set; }

    /// <summary>
    ///     Additional CSS classes applied to each mark's label.
    /// </summary>
    public string? MarkLabel { get; set; }

    /// <summary>
    ///     Additional CSS classes applied to the container that holds the marks.
    /// </summary>
    public string? Marks { get; set; }

    /// <summary>
    ///     Additional CSS classes applied to the value shown beside the label.
    /// </summary>
    public string? Value { get; set; }
}
