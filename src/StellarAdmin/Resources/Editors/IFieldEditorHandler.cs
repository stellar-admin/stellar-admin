namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Prepares the data that a field editor renders on the current request.
/// </summary>
public interface IFieldEditorHandler
{
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
public interface IFieldEditorHandler<in TEditor> : IFieldEditorHandler
    where TEditor : FieldEditor;
