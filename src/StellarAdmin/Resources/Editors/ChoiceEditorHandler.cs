namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Loads the choices of a choice editor for each request.
/// </summary>
/// <typeparam name="TEditor">The type of the editor.</typeparam>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public abstract class ChoiceEditorHandler<TEditor>(TEditor editor, IServiceProvider services)
    : FieldEditorHandler<TEditor>(editor)
    where TEditor : ChoiceEditor
{
    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) =>
        Editor.ItemsLoader is { } itemsLoader
            ? await itemsLoader(services, cancellationToken)
            : null;
}
