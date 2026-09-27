using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Options;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceEditorTests
{
    [Test]
    public async Task SelectListEditor_UsesRegisteredProviderChoices()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields
                            .Add(product => product.Name)
                            .UseEditor<SelectListEditorOptions>(options =>
                                options.UseItems<FixedSelectListItemsProvider>()
                            );
                    })
                ),
            dashboard => dashboard.Services.AddScoped<FixedSelectListItemsProvider>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product/Create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.Name'] option[value='notebook']")
                    .TextContent
            )
            .IsEqualTo("Notebook");
    }

    [Test]
    public async Task SelectListEditor_UsesSnapshotOfConfiguredChoices()
    {
        // Arrange
        var choices = new List<SelectListItem> { new("Notebook", "notebook") };
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields
                            .Add(product => product.Name)
                            .UseEditor<SelectListEditorOptions>(options =>
                                options.UseItems(choices)
                            );
                        fields.Add(product => product.Price);
                    })
                )
        );
        using var client = sut.GetTestClient();
        var firstDocument = await client.GetDocumentAsync("/stellaradmin/Product/Create");
        choices.Add(new("Pen", "pen"));

        // Act
        var secondDocument = await client.GetDocumentAsync("/stellaradmin/Product/Create");

        // Assert
        await Assert
            .That(firstDocument.QuerySelector("select[name='Entity.Name'] option[value='pen']"))
            .IsNull();
        await Assert
            .That(secondDocument.QuerySelector("select[name='Entity.Name'] option[value='pen']"))
            .IsNull();
    }

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
                            .UseEditor<ProductNameEditorOptions>(options =>
                                options.Placeholder = "Product name"
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
        await Assert
            .That(input.GetAttribute("data-editor-context"))
            .IsEqualTo("Product name:Development");
    }
}
