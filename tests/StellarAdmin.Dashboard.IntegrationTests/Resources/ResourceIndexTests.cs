using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceIndexTests
{
    [Test]
    public async Task EmptyDataSource_RendersEmptyStateAndColumnHeaders()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var html = await client.GetStringAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(html);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='empty-title']")?.TextContent.Trim())
            .IsEqualTo("No records found");
        await Assert
            .That(
                document
                    .QuerySelectorAll("thead th")
                    .Select(element => element.TextContent.Trim())
                    .ToArray()
            )
            .IsEquivalentTo(["Product name", "Unit price"]);
    }

    [Test]
    public async Task Products_RendersConfiguredColumnsAndEncodedValues()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(1, "<script>alert('test')</script>", 12.5m)])
        );
        using var client = sut.GetTestClient();

        // Act
        var html = await client.GetStringAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(html);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='page-header-title']")?.TextContent.Trim())
            .IsEqualTo("Products");
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(1);
        await Assert
            .That(document.QuerySelector("tbody td")?.TextContent.Trim())
            .IsEqualTo("<script>alert('test')</script>");
        await Assert.That(document.QuerySelector("tbody script")).IsNull();
        await Assert
            .That(document.QuerySelectorAll("tbody td")[1].TextContent.Trim())
            .IsEqualTo("12.50");
        await Assert
            .That(
                document.QuerySelector("a[href*='/Edit'], [hx-post], [data-slot='data-grid-pager']")
            )
            .IsNull();
    }

    [Test]
    public async Task UnregisteredResource_ReturnsNotFound()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Unknown");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}
