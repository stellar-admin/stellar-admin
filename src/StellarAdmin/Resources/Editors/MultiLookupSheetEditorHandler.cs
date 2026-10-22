namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a multi-select lookup and resolves the selected items.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class MultiLookupSheetEditorHandler(
    MultiLookupSheetEditor editor,
    IServiceProvider services
) : FieldEditorHandler<MultiLookupSheetEditor>(editor)
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <c>UseItems</c> wasn't called on the editor.
    /// </exception>
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        // A resource that can't be referred to has been reported, and leaves the lookup without items
        if (((ILookupSheetEditor)Editor).ResolveItems(services) is not { } items)
        {
            return new MultiLookupSheetEditorData([], false);
        }

        return new MultiLookupSheetEditorData(
            await items.FindAsync(services, context, cancellationToken),
            items.MediaType is not null
        );
    }
}
