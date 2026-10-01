namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Loads choices for a field editor. Without items, the template takes them from the property.
/// </summary>
public abstract class ChoiceEditorHandler<TEditor>(TEditor editor, IServiceProvider services)
    : FieldEditorHandler<TEditor>(editor)
    where TEditor : ChoiceEditor
{
    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(CancellationToken cancellationToken) =>
        Editor.ItemsLoader is { } itemsLoader
            ? await itemsLoader(services, cancellationToken)
            : null;
}
