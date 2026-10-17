namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a form editor.
/// </summary>
/// <remarks>
///     A custom editor derives from this class and implements
///     <see cref="IFieldEditor{THandler}" />.
/// </remarks>
public class FieldEditor
{
    // Future editor-specific options can add behavior such as masks and choice sources.

    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public virtual EditorClassNames ClassNames { get; } = new();
}
