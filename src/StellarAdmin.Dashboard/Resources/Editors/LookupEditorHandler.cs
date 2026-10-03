namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a lookup and resolves the selected item.
/// </summary>
public sealed class LookupEditorHandler(LookupEditor editor, IServiceProvider services)
    : FieldEditorHandler<LookupEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Lookup";

    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        var items =
            Editor.Items
            ?? throw new InvalidOperationException(
                $"LookupEditor on {context.FieldName} requires UseItems."
            );

        return await items.FindAsync(services, context, cancellationToken);
    }
}
