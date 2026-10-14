using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Infrastructure;

namespace StellarAdmin.IntegrationTests.Layout;

public class StellarAdminSheetTests
{
    [Test]
    public async Task ConfiguredLabels_ReplaceErrorText()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureResourceLabels(labels =>
                    labels.Sheet(sheet =>
                    {
                        sheet.ErrorTitle = "Could not open";
                        sheet.ErrorDescription = "Please try once more.";
                        sheet.RetryLabel = "Reload";
                    })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var sheet = (
            (IHtmlTemplateElement)document.RequiredElement("template#admin-sheet-template")
        ).Content;
        var error = (
            (IHtmlTemplateElement)sheet.RequiredElement("#admin-sheet template[data-sheet='error']")
        ).Content;
        await Assert
            .That(error.QuerySelector("[data-slot='empty-title']")?.TextContent)
            .IsEqualTo("Could not open");
        await Assert
            .That(error.QuerySelector("[data-slot='empty-description']")?.TextContent)
            .IsEqualTo("Please try once more.");
        await Assert
            .That(error.QuerySelector("[data-sheet='retry']")?.TextContent)
            .IsEqualTo("Reload");
    }

    [Test]
    public async Task Layout_RendersSheetLevelTemplate()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.QuerySelector("admin-remote-sheet")).IsNull();
        var template = (
            (IHtmlTemplateElement)document.RequiredElement("template#admin-sheet-template")
        ).Content;
        var sheet = template.RequiredElement("admin-remote-sheet > sel-dialog > #admin-sheet");
        await Assert
            .That(sheet.RequiredElement("[data-sheet='content']").Id)
            .IsEqualTo("admin-sheet-content");
        await Assert.That(sheet.QuerySelector("template[data-sheet='error']")).IsNotNull();
    }
}
