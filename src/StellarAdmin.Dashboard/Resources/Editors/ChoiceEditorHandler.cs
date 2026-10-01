namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Loads choices for a field editor.
/// </summary>
public abstract class ChoiceEditorHandler<TEditor>(TEditor editor, IServiceProvider services)
    : FieldEditorHandler<TEditor>(editor)
    where TEditor : ChoiceEditor
{
    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(CancellationToken cancellationToken)
    {
        var itemsLoader =
            Editor.ItemsLoader
            ?? throw new InvalidOperationException("Choice editor items are required.");

        return await itemsLoader(services, cancellationToken);
    }
}
