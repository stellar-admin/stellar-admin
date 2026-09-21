using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourcePagingTests
{
    [Test]
    [Arguments(false, 3)]
    [Arguments(true, 1)]
    public async Task BeyondLastPage_RedirectsToLastPage(bool empty, int lastPage)
    {
        // Arrange
        var state = empty ? new ProductState([]) : CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, EnablePaging);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?page=9&pageSize=2");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo($"/stellaradmin/Product?page={lastPage}&pageSize=2");
        var document = await client.GetDocumentAsync(response.Headers.Location!.OriginalString);
        await Assert
            .That(document.QuerySelector("[data-slot='empty-title']") is not null)
            .IsEqualTo(empty);
    }

    [Test]
    public async Task DefaultPaging_UsesDefaultPageAndSizes()
    {
        // Arrange
        var state = new ProductState(
            Enumerable.Range(1, 26).Select(id => new Product(id, $"Product {id}", id)).ToArray()
        );
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => resource.Index(index => index.EnablePaging())
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert.That(state.ListRequests.Single().Paging).IsEqualTo(new ResourcePaging(1, 25));
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(25);
        await Assert
            .That(document.TextContents("[data-slot='data-grid-page-size'] a"))
            .IsEquivalentTo(["10", "25", "50", "100"]);
    }

    [Test]
    public async Task DeletingLastRowOnPage_ReturnsToPreviousPage()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                EnablePaging(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        var document = await client.GetDocumentAsync("/stellaradmin/Product?page=3&pageSize=2");
        var action = document.RequiredElement("form[data-resource-delete]").GetAttribute("action")!;
        using var content = new FormUrlEncodedContent(
            await PrepareForm(client, "/stellaradmin/Product?page=3&pageSize=2")
        );

        // Act
        using var response = await client.PostAsync(action, content);

        // Assert
        await Assert.That(action).IsEqualTo("/stellaradmin/Product/Delete/5?page=3&pageSize=2");
        await Assert.That(state.Products.Count).IsEqualTo(4);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=3&pageSize=2");
        using var index = await client.GetAsync(response.Headers.Location!.OriginalString);
        await Assert
            .That(index.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=2&pageSize=2");
        var lastPage = await client.GetDocumentAsync(index.Headers.Location!.OriginalString);
        await Assert
            .That(lastPage.RequiredElement("tbody td").TextContent.Trim())
            .IsEqualTo("Product 3");
    }

    [Test]
    public async Task DisabledPaging_IgnoresQueryAndReturnsAllRows()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product?page=2&pageSize=1");

        // Assert
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(5);
        await Assert.That(document.QuerySelector("[data-slot='data-grid-pager']")).IsNull();
        await Assert.That(state.ListRequests.Single().Paging).IsNull();
        await Assert.That(document.QuerySelector(".validation-summary-errors")).IsNull();
    }

    [Test]
    [Arguments("page=invalid")]
    [Arguments("pageSize=invalid")]
    public async Task DisabledPaging_WithBindingErrors_ReturnsBadRequest(string query)
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?" + query);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.ListRequests.Count).IsEqualTo(0);
    }

    [Test]
    [Arguments("page=0")]
    [Arguments("page=-1")]
    [Arguments("page=invalid")]
    [Arguments("page=2147483648")]
    [Arguments("page=2147483647")]
    [Arguments("pageSize=invalid")]
    public async Task InvalidPaging_ReturnsBadRequestWithoutLoading(string query)
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, EnablePaging);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?" + query);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.ListRequests.Count).IsEqualTo(0);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RejectedDelete_PreservesErrorsAndAdjustsPageIfNeeded(bool pageDisappeared)
    {
        // Arrange
        var state = CreateState();
        state.DeleteResult = ResourceOperationResult.ValidationFailed(null, "Still in use.");
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                EnablePaging(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(
            await PrepareForm(client, "/stellaradmin/Product?page=3&pageSize=2")
        );
        if (pageDisappeared)
        {
            state.Products.RemoveAll(product => product.Id == 1);
        }

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/Product/Delete/5?page=3&pageSize=2",
            content
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .Contains("Still in use.");
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='pagination-link'][data-active='true']")
                    .TextContent.Trim()
            )
            .IsEqualTo(pageDisappeared ? "2" : "3");
        await Assert.That(document.QuerySelector("form[action*='/Delete/5']")).IsNotNull();
    }

    [Test]
    [Arguments("", 1, 2, "Product 1", "1–2 of 5")]
    [Arguments("?page=2", 2, 2, "Product 3", "3–4 of 5")]
    [Arguments("?page=2&pageSize=3", 2, 3, "Product 4", "4–5 of 5")]
    [Arguments("?pageSize=999", 1, 2, "Product 1", "1–2 of 5")]
    [Arguments("?pageSize=0", 1, 2, "Product 1", "1–2 of 5")]
    public async Task RequestedPage_RendersSourceRowsTotalsAndNavigation(
        string query,
        int page,
        int pageSize,
        string firstName,
        string range
    )
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, EnablePaging);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product" + query);

        // Assert
        await Assert
            .That(state.ListRequests.Single().Paging)
            .IsEqualTo(new ResourcePaging(page, pageSize));
        await Assert
            .That(document.RequiredElement("tbody td").TextContent.Trim())
            .IsEqualTo(firstName);
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(2);
        await Assert
            .That(document.RequiredElement("[data-slot='data-grid-range']").TextContent.Trim())
            .IsEqualTo(range);
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='pagination-link'][data-active='true']")
                    .TextContent.Trim()
            )
            .IsEqualTo(page.ToString());
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='data-grid-pager'] a[href]")
                    .GetAttribute("href")
            )
            .Contains($"pageSize={pageSize}");
        await Assert
            .That(
                document.QuerySelector(
                    "[data-slot='data-grid-page-size'] a[href='/stellaradmin/Product?page=1&pageSize=3']"
                )
            )
            .IsNotNull();
    }

    [Test]
    [Arguments(0, "0 records")]
    [Arguments(1, "1 record")]
    public async Task SinglePage_RendersCountAndSizeSelectorWithoutNavigation(
        int count,
        string summary
    )
    {
        // Arrange
        var state = new ProductState(CreateState().Products.Take(count).ToArray());
        await using var sut = await DashboardTestHost.CreateAsync(state, EnablePaging);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='data-grid-range']").TextContent.Trim())
            .IsEqualTo(summary);
        await Assert.That(document.QuerySelector("[data-slot='data-grid-page-size']")).IsNotNull();
        await Assert.That(document.QuerySelector("[data-slot='data-grid-pager']")).IsNull();
    }

    private static ProductState CreateState() =>
        new(
            Enumerable
                .Range(1, 5)
                .Select(id => new Product(id, $"Product {id}", id))
                .Reverse()
                .ToArray()
        );

    private static void EnablePaging(ResourceBuilder<Product> resource) =>
        resource.Index(index =>
            index.EnablePaging(paging =>
            {
                paging.PageSize = 2;
                paging.PageSizes = [2, 3];
            })
        );
}
