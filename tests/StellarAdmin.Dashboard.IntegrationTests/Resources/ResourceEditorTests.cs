using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceEditorTests
{
    [Test]
    public async Task CustomEditor_RendersApplicationTemplateWithTypedSettings()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<CreateProductModel, CreateProductHandler>(create =>
                {
                    create.UseFactory(() => new(10m));
                    create.Fields(fields =>
                        fields
                            .Add(model => model.ProductName)
                            .UseEditor<ProductNameEditor>(editor =>
                                editor.Placeholder = "Product name"
                            )
                    );
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product/Create");

        // Assert
        var input = document.RequiredElement("input[data-custom-editor='product-name']");
        await Assert.That(input.GetAttribute("name")).IsEqualTo("Entity.ProductName");
        await Assert.That(input.GetAttribute("placeholder")).IsEqualTo("Product name");
    }
}
