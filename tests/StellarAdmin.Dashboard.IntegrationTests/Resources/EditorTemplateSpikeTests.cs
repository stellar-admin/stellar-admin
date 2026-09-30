using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

// Spike: verifies type-name templates can delegate to a friendly editor via a partial.
public class EditorTemplateSpikeTests
{
    [Test]
    public async Task StringTemplate_DelegatesToTextInput_PreservesMetadata()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[name='Entity.Name']");
        await Assert.That(input.GetAttribute("data-spike-editor")).IsEqualTo("text-input");
        await Assert.That(input.GetAttribute("id")).IsEqualTo("Entity_Name");
        await Assert.That(input.GetAttribute("data-val-required")).IsNotNull();
        await Assert
            .That(document.RequiredElement("label[for='Entity_Name']").TextContent.Trim())
            .IsEqualTo("Product name");
    }

    [Test]
    public async Task DecimalTemplate_DelegatesToTextInput_InfersNumberAndPreservesRange()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[name='Entity.Price']");
        await Assert.That(input.GetAttribute("data-spike-editor")).IsEqualTo("text-input");
        await Assert.That(input.GetAttribute("type")).IsEqualTo("number");
        await Assert.That(input.GetAttribute("step")).IsEqualTo("any");
        await Assert.That(input.GetAttribute("data-val-range")).IsNotNull();
    }

    [Test]
    public async Task DelegatedTemplate_InvalidSubmission_ShowsFieldError()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "";
        values["Entity.Price"] = "0";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.RequiredElement("[data-valmsg-for='Entity.Name']").TextContent.Trim())
            .IsNotEmpty();
        await Assert
            .That(document.RequiredElement("[data-valmsg-for='Entity.Price']").TextContent.Trim())
            .IsNotEmpty();
        await Assert
            .That(document.RequiredElement("input[name='Entity.Price']").GetAttribute("value"))
            .IsEqualTo("0");
    }

    [Test]
    public async Task UseEditor_TemplateNameInSubfolder_RendersFriendlyEditor()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<CreateProductModel, CreateProductHandler>(create =>
                {
                    create.UseFactory(() => new(10m));
                    create.Fields(fields =>
                    {
                        fields.Add(model => model.ProductName).UseEditor<SpikeTextInputEditorOptions>();
                        fields.Add(model => model.Password).UseEditor<SpikeTextInputEditorOptions>();
                    });
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var name = document.RequiredElement("input[name='Entity.ProductName']");
        await Assert.That(name.GetAttribute("data-spike-editor")).IsEqualTo("text-input");
        await Assert.That(name.GetAttribute("data-val-required")).IsNotNull();
        var password = document.RequiredElement("input[name='Entity.Password']");
        await Assert.That(password.GetAttribute("type")).IsEqualTo("password");
    }
}
