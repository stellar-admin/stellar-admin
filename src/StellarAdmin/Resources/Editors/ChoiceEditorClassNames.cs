namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores additional CSS classes for the parts of a choice editor.
/// </summary>
public class ChoiceEditorClassNames : EditorClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for each choice's media.
    /// </summary>
    /// <remarks>
    ///     A size utility replaces the default size.
    /// </remarks>
    public string? Media { get; set; }
}
