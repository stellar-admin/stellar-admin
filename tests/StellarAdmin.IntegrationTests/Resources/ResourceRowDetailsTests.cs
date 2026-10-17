using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.UI;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceRowDetailsTests
{
    [Test]
    public async Task InlineDetails_RenderPartialCollapsedBelowEachRow()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
                resource.Index(index =>
                    index.EnableRowDetails(
                        "_ProductSummary",
                        details =>
                        {
                            details.RowClick = true;
                            details.ExpandMode = DataGridRowDetailExpandMode.Single;
                            details.Emphasis = TableRowDetailEmphasis.Rail;
                            details.Divider = TableRowDetailDivider.FullWidth;
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
        var rowDetails = document.RequiredElement("[data-slot='table-interactivity']");
        await Assert.That(rowDetails.GetAttribute("data-row-click")).IsEqualTo("true");
        await Assert.That(rowDetails.GetAttribute("data-expand-mode")).IsEqualTo("single");
        await Assert.That(rowDetails.GetAttribute("data-emphasis")).IsEqualTo("rail");
        await Assert.That(rowDetails.GetAttribute("data-divider")).IsEqualTo("full-width");
        await Assert.That(rowDetails.GetAttribute("data-inset")).IsEqualTo("bleed");
        await Assert.That(rowDetails.GetAttribute("data-animate")).IsEqualTo("false");
        await Assert.That(document.QuerySelector("[data-slot='table-row-detail-toggle']")).IsNull();
    }

    [Test]
    public async Task HandlerDetails_RenderLoaderForTheRowKey()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
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
        await Assert.That(loader.GetAttribute("hx-trigger")).Contains("rowdetailexpand");
        await Assert.That(document.QuerySelector("[data-test='notes']")).IsNull();
    }

    [Test]
    public async Task HandlerDetails_RowDetailRendersHandlerModel()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
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

    [Test]
    public async Task Panels_RenderTabsForEachRow()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m), new(8, "Pencil", 1.25m)]),
            ConfigurePanels
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var content = document.QuerySelectorAll("[data-slot='table-row-detail-content']")[1];
        await Assert
            .That(content.TextContents("[role='tab']"))
            .IsEquivalentTo(["Notes", "Summary", "Archived notes"]);
        await Assert
            .That(content.QuerySelectorAll("[role='tabpanel']").Select(panel => panel.Id))
            .IsEquivalentTo([
                "--resource-row-detail-2-notes",
                "--resource-row-detail-2-summary",
                "--resource-row-detail-2-archived-notes",
            ]);
        await Assert
            .That(content.RequiredElement("[data-test='summary']").TextContent)
            .IsEqualTo("Summary for Pencil");
    }

    [Test]
    public async Task Panels_LoadFirstPanelOnExpandAndOthersWhenShown()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePanels
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var notes = document.RequiredElement("#--resource-row-detail-1-notes");
        await Assert
            .That(notes.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/rowdetail/7?panel=notes");
        await Assert
            .That(notes.GetAttribute("hx-trigger"))
            .IsEqualTo("rowdetailexpand from:\"closest tr\" once");
        var archived = document.RequiredElement("#--resource-row-detail-1-archived-notes");
        await Assert
            .That(archived.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/rowdetail/7?panel=archived-notes");
        await Assert.That(archived.GetAttribute("hx-trigger")).IsEqualTo("tabshow once");
        await Assert
            .That(
                document.RequiredElement("#--resource-row-detail-1-summary").HasAttribute("hx-get")
            )
            .IsFalse();
    }

    [Test]
    public async Task Panels_RowDetailRendersNamedPanel()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePanels
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/rowdetail/7?panel=archived-notes"
        );

        // Assert
        await Assert
            .That(document.RequiredElement("[data-test='notes']").TextContent)
            .IsEqualTo("Notes for Notebook");
    }

    [Test]
    [Arguments("")]
    [Arguments("?panel=summary")]
    [Arguments("?panel=missing")]
    public async Task Panels_RowDetailWithoutOnDemandPanel_ReturnsNotFound(string query)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePanels
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/rowdetail/7{query}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(await response.Content.ReadAsStringAsync()).IsEmpty();
    }

    [Test]
    public async Task SinglePanel_RendersWithoutTabs()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
                resource.Index(index =>
                    index.EnableRowDetails(panels => panels.Add("Summary", "_ProductSummary"))
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.QuerySelector("sel-tabs")).IsNull();
        await Assert
            .That(document.RequiredElement("[data-test='summary']").TextContent)
            .IsEqualTo("Summary for Notebook");
    }

    private static void ConfigurePanels(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.Index(index =>
            index.EnableRowDetails(panels =>
            {
                panels.Add<ProductNotes, ProductNotesHandler>("Notes", "_ProductNotes");
                panels.Add("Summary", "_ProductSummary");
                panels.Add<ProductNotes, ProductNotesHandler>("Archived notes", "_ProductNotes");
            })
        );
    }

    private static void ConfigureHandlerDetails(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.Index(index =>
            index.EnableRowDetails<ProductNotes, ProductNotesHandler>("_ProductNotes")
        );
    }
}
