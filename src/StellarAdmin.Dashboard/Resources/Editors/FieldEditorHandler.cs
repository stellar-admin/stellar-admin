namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Base class for editor handlers that need no request data.
/// </summary>
public abstract class FieldEditorHandler<TEditor>(TEditor editor) : IFieldEditorHandler<TEditor>
    where TEditor : FieldEditor
{
    /// <summary>
    ///     The editor's configuration.
    /// </summary>
    protected TEditor Editor { get; } = editor;

    /// <inheritdoc />
    public abstract string TemplateName { get; }

    /// <inheritdoc />
    public virtual Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) => Task.FromResult<object?>(null);
}
