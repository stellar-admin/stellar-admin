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
        var error = (
            (IHtmlTemplateElement)
                document.RequiredElement("#dashboard-sheet template[data-sheet='error']")
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
    public async Task Layout_RendersSharedAndNestedSheets()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        foreach (var id in new[] { "dashboard-sheet", "dashboard-nested-sheet" })
        {
            var sheet = document.RequiredElement($"dashboard-remote-sheet > sel-dialog > #{id}");
            await Assert
                .That(sheet.RequiredElement("[data-sheet='content']").Id)
                .IsEqualTo($"{id}-content");
            await Assert.That(sheet.QuerySelector("template[data-sheet='error']")).IsNotNull();
        }
    }
}
