using StellarAdmin.Resources.Editors;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductNameEditor : FieldEditor, IFieldEditor<ProductNameEditorHandler>
{
    public string Placeholder { get; set; } = "";
}
