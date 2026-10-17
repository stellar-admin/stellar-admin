namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Prepares a form field for rendering with an editor template.
/// </summary>
public interface IFieldEditorHandler
{
    /// <summary>
    ///     Gets the name of the editor template that renders the field.
    /// </summary>
    /// <value>
    ///     The template name, relative to <c>EditorTemplates</c>, such as
    ///     <c>Editors/TextInput</c>.
    /// </value>
    string TemplateName { get; }

    /// <summary>
    ///     Loads the data the editor template needs to render the field.
    /// </summary>
    /// <param name="context">The field to prepare.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     The data the template reads from <c>EditorData</c>, or <see langword="null" /> for none.
    /// </returns>
    Task<object?> PrepareAsync(FieldEditorContext context, CancellationToken cancellationToken);
}

/// <summary>
///     Prepares a form field configured by the specified editor.
/// </summary>
/// <typeparam name="TEditor">The type of the editor.</typeparam>
public interface IFieldEditorHandler<TEditor> : IFieldEditorHandler
    where TEditor : FieldEditor;
