using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceRowDetailsTests
{
    [Test]
    public async Task InlineDetails_RenderPartialCollapsedBelowEachRow()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m), new(8, "Pencil", 1.25m)]),
            resource => resource.Index(index => index.EnableRowDetails("_ProductSummary"))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var detailRows = document.QuerySelectorAll("tr[data-slot='table-row-detail']");
        await Assert.That(detailRows.Length).IsEqualTo(2);
        await Assert.That(detailRows.All(row => row.HasAttribute("hidden"))).IsTrue();
        await Assert
            .That(document.TextContents("[data-test='summary']"))
            .IsEquivalentTo(["Summary for Notebook", "Summary for Pencil"]);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='table-row-detail-toggle']:not([data-all])")
                    .Length
            )
            .IsEqualTo(2);
    }

    [Test]
    public async Task ConfiguredSettings_RenderOnRowDetailsElement()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
                resource.Index(index =>
                    index.EnableRowDetails(
                        "_ProductSummary",
                        details =>
                        {
                            details.RowClick = true;
                            details.ExpandMode = TableRowDetailExpandMode.Single;
                            details.Emphasis = TableRowDetailEmphasis.Rail;
                            details.Inset = TableRowDetailInset.Bleed;
                            details.Animate = false;
                            details.Toggle = DataGridRowDetailToggle.None;
                        }
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var rowDetails = document.RequiredElement("[data-slot='table-row-details']");
        await Assert.That(rowDetails.GetAttribute("data-row-click")).IsEqualTo("true");
        await Assert.That(rowDetails.GetAttribute("data-expand-mode")).IsEqualTo("single");
        await Assert.That(rowDetails.GetAttribute("data-emphasis")).IsEqualTo("rail");
        await Assert.That(rowDetails.GetAttribute("data-inset")).IsEqualTo("bleed");
        await Assert.That(rowDetails.GetAttribute("data-animate")).IsEqualTo("false");
        await Assert.That(document.QuerySelector("[data-slot='table-row-detail-toggle']")).IsNull();
    }

    [Test]
    public async Task HandlerDetails_RenderLoaderForTheRowKey()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureHandlerDetails
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var loader = document.RequiredElement("[data-slot='table-row-detail-content'] [hx-get]");
        await Assert
            .That(loader.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/rowdetail/7");
        await Assert.That(loader.GetAttribute("hx-trigger")).Contains("row-detail-expand");
        await Assert.That(document.QuerySelector("[data-test='notes']")).IsNull();
    }

    [Test]
    public async Task HandlerDetails_RowDetailRendersHandlerModel()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureHandlerDetails
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/rowdetail/7");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-test='notes']").TextContent)
            .IsEqualTo("Notes for Notebook");
        await Assert.That(document.QuerySelector("[data-slot='sidebar']")).IsNull();
    }

    [Test]
    public async Task HandlerDetails_MissingRecord_ReturnsNotFoundMessage()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureHandlerDetails
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/rowdetail/99");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("This record no longer exists.");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task DetailsNotLoadedOnDemand_RowDetailReturnsNotFound(bool inlineDetails)
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                if (inlineDetails)
                {
                    resource.Index(index => index.EnableRowDetails("_ProductSummary"));
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/rowdetail/7");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(await response.Content.ReadAsStringAsync()).IsEmpty();
    }

    [Test]
    public async Task HandlerDetailsWithoutKey_IndexFailsWithKeyMessage()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
                resource.Index(index =>
                    index.EnableRowDetails<ProductNotes, ProductNotesHandler>("_ProductNotes")
                )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("require a resource key");
    }

    [Test]
    public async Task RequiredAuthorization_AnonymousRowDetailRequest_ReturnsUnauthorized()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                ConfigureHandlerDetails(resource);
                resource.RequireAuthorization();
            },
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/rowdetail/7");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    private static void ConfigureHandlerDetails(
        Dashboard.Resources.Builders.ResourceBuilder<Product> resource
    )
    {
        resource.UseKey(product => product.Id);
        resource.Index(index =>
            index.EnableRowDetails<ProductNotes, ProductNotesHandler>("_ProductNotes")
        );
    }
}
