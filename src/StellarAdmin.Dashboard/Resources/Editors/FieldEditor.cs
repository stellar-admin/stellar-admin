namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures the appearance of a form editor.
/// </summary>
public class FieldEditor
{
    // Future editor-specific options can add behavior such as masks and choice sources.

    /// <summary>
    ///     Additional CSS classes for the editor's parts.
    /// </summary>
    public virtual EditorClassNames ClassNames { get; } = new();
}
