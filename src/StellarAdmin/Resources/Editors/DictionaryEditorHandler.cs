namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Loads the items of a dictionary editor's keys and values for each request.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class DictionaryEditorHandler(DictionaryEditor editor, IServiceProvider services)
    : FieldEditorHandler<DictionaryEditor>(editor)
{
    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) =>
        new DictionaryEditorData(
            await LoadAsync(Editor.KeyPart, context, cancellationToken),
            await LoadAsync(Editor.ValuePart, context, cancellationToken)
        );

    private async Task<IReadOnlyList<ChoiceItem>?> LoadAsync(
        DictionaryEditorPart part,
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) =>
        part.Source.ItemsLoader is { } itemsLoader
            ? await itemsLoader(context.Model, services, cancellationToken)
            : null;
}

// The items of a dictionary editor's keys and values, or null for a part without items
internal sealed record DictionaryEditorData(
    IReadOnlyList<ChoiceItem>? Keys,
    IReadOnlyList<ChoiceItem>? Values
);
