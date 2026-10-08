namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a multi-select lookup and resolves the selected items.
/// </summary>
public sealed class MultiLookupSheetEditorHandler(
    MultiLookupSheetEditor editor,
    IServiceProvider services
) : FieldEditorHandler<MultiLookupSheetEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/MultiLookupSheet";

    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        var items =
            Editor.Items
            ?? throw new InvalidOperationException(
                $"MultiLookupSheetEditor on {context.FieldName} requires UseItems."
            );

        return new MultiLookupSheetEditorData(
            await items.FindAsync(services, context, cancellationToken)
        );
    }
}
