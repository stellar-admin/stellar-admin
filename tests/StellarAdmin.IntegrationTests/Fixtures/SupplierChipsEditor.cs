using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.IntegrationTests.Fixtures;

// An application's own editor that takes its items from a resource
public sealed class SupplierChipsEditor : FieldEditor, IFieldEditor<SupplierChipsEditorHandler>
{
    public ReferencedResource? Resource { get; private set; }

    public override string TemplateName => nameof(SupplierChipsEditor);

    public void UseResource<TResource>() => Resource = ReferencedResource.For<TResource>();
}

public sealed class SupplierChipsEditorHandler(
    SupplierChipsEditor editor,
    ResourceReferences references,
    IServiceProvider services
) : FieldEditorHandler<SupplierChipsEditor>(editor)
{
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) =>
        Editor.Resource is { } resource && references.GetItems(resource) is { } items
            ? await items.ListAsync(services, context.Model, cancellationToken)
            : null;
}
