using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductNameEditor : ResourceEditor
{
    public string Placeholder { get; set; } = "";
}
