namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Provides a base class for editor handlers.
/// </summary>
/// <typeparam name="TEditor">The type of the editor.</typeparam>
/// <param name="editor">The editor's configuration.</param>
public abstract class FieldEditorHandler<TEditor>(TEditor editor) : IFieldEditorHandler<TEditor>
    where TEditor : FieldEditor
{
    /// <summary>
    ///     Gets the editor's configuration.
    /// </summary>
    protected TEditor Editor { get; } = editor;

    /// <inheritdoc />
    public abstract Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    );
}
