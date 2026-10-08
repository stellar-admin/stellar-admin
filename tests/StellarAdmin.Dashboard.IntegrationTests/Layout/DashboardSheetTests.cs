using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Layout;

public class DashboardSheetTests
{
    [Test]
    public async Task ConfiguredLabels_ReplaceErrorText()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.ConfigureResourceLabels(labels =>
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
            (IHtmlTemplateElement)document.RequiredElement("template#dashboard-sheet-template")
        ).Content;
        var error = (
            (IHtmlTemplateElement)
                sheet.RequiredElement("#dashboard-sheet template[data-sheet='error']")
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
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.QuerySelector("dashboard-remote-sheet")).IsNull();
        var template = (
            (IHtmlTemplateElement)document.RequiredElement("template#dashboard-sheet-template")
        ).Content;
        var sheet = template.RequiredElement(
            "dashboard-remote-sheet > sel-dialog > #dashboard-sheet"
        );
        await Assert
            .That(sheet.RequiredElement("[data-sheet='content']").Id)
            .IsEqualTo("dashboard-sheet-content");
        await Assert.That(sheet.QuerySelector("template[data-sheet='error']")).IsNotNull();
    }
}
