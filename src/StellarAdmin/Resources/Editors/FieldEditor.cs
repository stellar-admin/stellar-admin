namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a form editor.
/// </summary>
/// <remarks>
///     Derive from this class to add settings. Render them in the editor template that the
///     derived class names, which inherits <c>FieldEditorView&lt;TEditor&gt;</c>. Implement
///     <see cref="IFieldEditor{THandler}" /> when the template needs data loaded for each request.
/// </remarks>
public abstract class FieldEditor
{
    /// <summary>
    ///     Gets the name of the editor template.
    /// </summary>
    /// <value>
    ///     The template name. The template is found at <c>EditorTemplates/{name}</c>, and at
    ///     <c>GridEditorTemplates/{name}</c> in index grid cells.
    /// </value>
    public abstract string TemplateName { get; }

    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public virtual EditorClassNames ClassNames { get; } = new();
}
