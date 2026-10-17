using StellarAdmin.Resources.Editors;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductCodeEditor : FieldEditor
{
    public string Prefix { get; set; } = "";

    public override string TemplateName => nameof(ProductCodeEditor);
}
