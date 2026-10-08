namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a lookup and resolves the selected item.
/// </summary>
public sealed class LookupSheetEditorHandler(LookupSheetEditor editor, IServiceProvider services)
    : FieldEditorHandler<LookupSheetEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/LookupSheet";

    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        var items =
            Editor.Items
            ?? throw new InvalidOperationException(
                $"LookupSheetEditor on {context.FieldName} requires UseItems."
            );

        var item = (await items.FindAsync(services, context, cancellationToken)).FirstOrDefault();
        var createController = Editor.CreateEnabled
            ? await LookupCreateResource.FindControllerAsync(
                services,
                nameof(LookupSheetEditor),
                context.FieldName,
                items,
                Editor.CreateResourceType
            )
            : null;

        return new LookupSheetEditorData(item, createController);
    }
}
