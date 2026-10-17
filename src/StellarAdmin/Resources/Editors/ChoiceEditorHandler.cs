namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Loads the choices of a choice editor for each request.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class ChoiceEditorHandler(ChoiceEditor editor, IServiceProvider services)
    : FieldEditorHandler<ChoiceEditor>(editor)
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
