using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductNameEditorOptions : EditorOptions, IFieldEditorOptions<ProductNameEditor>
{
    public string Placeholder { get; set; } = "";
}
