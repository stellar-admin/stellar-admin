using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceSortingTests
{
    [Test]
    [Arguments("", "Name", ResourceSortDirection.Ascending, "Alpha")]
    [Arguments(
        "?sortBy=price&sortDirection=desc",
        "Price",
        ResourceSortDirection.Descending,
        "Bravo"
    )]
    [Arguments("?sortBy=Id&sortDirection=desc", "Name", ResourceSortDirection.Ascending, "Alpha")]
    [Arguments("?sortBy=unknown", "Name", ResourceSortDirection.Ascending, "Alpha")]
    public async Task SelectedSort_OrdersBeforePagingAndMarksHeader(
        string query,
        string field,
        ResourceSortDirection direction,
        string firstName
    )
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureSorting);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product" + query);

        // Assert
        await Assert
            .That(state.ListRequests.Single().Sort)
            .IsEqualTo(new ResourceSort(field, direction));
        await Assert
            .That(document.RequiredElement("tbody td").TextContent.Trim())
            .IsEqualTo(firstName);
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(2);
        await Assert
            .That(document.RequiredElement("th[aria-sort]").GetAttribute("aria-sort"))
            .IsEqualTo(direction == ResourceSortDirection.Ascending ? "ascending" : "descending");
        await Assert
            .That(document.QuerySelectorAll("[data-slot='data-grid-sort-link']").Length)
            .IsEqualTo(2);
    }

    [Test]
    public async Task SortLinks_ResetPageAndPreserveSizeWhilePagerPreservesSort()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(CreateState(), ConfigureSorting);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/Product?page=2&pageSize=2&sortBy=Price&sortDirection=desc"
        );
        var sortLink = document
            .RequiredElement("[data-slot='data-grid-sort-link'][data-active='true']")
            .GetAttribute("href")!;
        var sorted = await client.GetDocumentAsync(sortLink);

        // Assert
        await Assert.That(sortLink).Contains("sortDirection=asc");
        await Assert.That(sortLink).Contains("pageSize=2");
        await Assert.That(sortLink.Contains("page=2")).IsFalse();
        await Assert.That(sorted.RequiredElement("tbody td").TextContent.Trim()).IsEqualTo("Zulu");
        foreach (
            var link in document.QuerySelectorAll(
                "[data-slot='data-grid-pager'] a[href], [data-slot='data-grid-page-size'] a[href]"
            )
        )
        {
            await Assert.That(link.GetAttribute("href")).Contains("sortBy=Price");
            await Assert.That(link.GetAttribute("href")).Contains("sortDirection=desc");
        }
    }

    [Test]
    public async Task EqualSortValues_KeepStableOrderAcrossPages()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureSorting);
        using var client = sut.GetTestClient();

        // Act
        var first = await client.GetDocumentAsync("/stellaradmin/Product?sortBy=Price");
        var second = await client.GetDocumentAsync("/stellaradmin/Product?sortBy=Price&page=2");

        // Assert
        await Assert
            .That(first.TextContents("tbody td:first-child"))
            .IsEquivalentTo(["Zulu", "Alpha"]);
        await Assert
            .That(second.TextContents("tbody td:first-child"))
            .IsEquivalentTo(["Charlie", "Bravo"]);
    }

    [Test]
    public async Task SortingWithoutPaging_UsesDescendingDefault()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
                resource.Index(index =>
                {
                    index.Columns(columns =>
                        columns
                            .Clear()
                            .Add(product => product.Name, column => column.Sortable = true)
                    );
                    index.DefaultSortByDescending(product => product.Name);
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert
            .That(state.ListRequests.Single().Sort)
            .IsEqualTo(new ResourceSort("Name", ResourceSortDirection.Descending));
        await Assert
            .That(document.RequiredElement("tbody td").TextContent.Trim())
            .IsEqualTo("Zulu");
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(4);
    }

    [Test]
    public async Task UnconfiguredSorting_IgnoresRequestedField()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/Product?sortBy=Name&sortDirection=desc"
        );

        // Assert
        await Assert.That(state.ListRequests.Single().Sort).IsNull();
        await Assert.That(document.QuerySelector("[data-slot='data-grid-sort-link']")).IsNull();
    }

    [Test]
    public async Task InvalidDirection_ReturnsBadRequestWithoutLoading()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureSorting);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/Product?sortBy=Name&sortDirection=invalid"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.ListRequests.Count).IsEqualTo(0);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Delete_PreservesSort(bool rejected)
    {
        // Arrange
        var state = CreateState();
        if (rejected)
        {
            state.DeleteResult = ResourceOperationResult.ValidationFailed(null, "Still in use.");
        }
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                ConfigureSorting(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        const string url =
            "/stellaradmin/Product?page=2&pageSize=2&sortBy=Price&sortDirection=desc";
        var document = await client.GetDocumentAsync(url);
        var action = document.RequiredElement("form[data-resource-delete]").GetAttribute("action")!;
        using var content = new FormUrlEncodedContent(await PrepareForm(client, url));

        // Act
        using var response = await client.PostAsync(action, content);

        // Assert
        await Assert.That(action).Contains("sortBy=Price");
        await Assert.That(action).Contains("sortDirection=desc");
        if (rejected)
        {
            var result = await response.ReadDocumentAsync();
            await Assert
                .That(result.RequiredElement(".validation-summary-errors").TextContent)
                .Contains("Still in use.");
            await Assert
                .That(result.RequiredElement("th[aria-sort]").GetAttribute("aria-sort"))
                .IsEqualTo("descending");
        }
        else
        {
            await Assert
                .That(response.Headers.Location?.OriginalString)
                .Contains("sortBy=Price&sortDirection=desc");
        }
    }

    [Test]
    public async Task OutOfRangePage_RedirectPreservesSort()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(CreateState(), ConfigureSorting);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/Product?page=9&sortBy=Price&sortDirection=desc"
        );

        // Assert
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=2&sortBy=Price&sortDirection=desc");
    }

    [Test]
    public async Task DefaultIndex_LinksOnlyIncludeSelectionsMadeByTheUser()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(CreateState(), ConfigureSorting);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");
        var nextLink = document
            .RequiredElement("a[aria-label='Go to next page']")
            .GetAttribute("href")!;
        var secondPage = await client.GetDocumentAsync(nextLink);

        // Assert
        await Assert.That(nextLink).IsEqualTo("/stellaradmin/Product?page=2");
        await Assert
            .That(secondPage.RequiredElement("th[aria-sort]").GetAttribute("aria-sort"))
            .IsEqualTo("ascending");
        await Assert
            .That(
                secondPage
                    .RequiredElement("[data-slot='data-grid-sort-link'][data-active='true']")
                    .GetAttribute("href")
            )
            .IsEqualTo("/stellaradmin/Product?sortBy=Name&sortDirection=desc");
        await Assert
            .That(
                document.RequiredElement("[data-slot='data-grid-page-size'] a").GetAttribute("href")
            )
            .IsEqualTo("/stellaradmin/Product?page=1&pageSize=2");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task DeleteWithoutSelections_DoesNotAddDefaults(bool rejected)
    {
        // Arrange
        var state = CreateState();
        if (rejected)
        {
            state.DeleteResult = ResourceOperationResult.ValidationFailed(null, "Still in use.");
        }

        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                ConfigureSorting(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        var document = await client.GetDocumentAsync("/stellaradmin/Product");
        var action = document.RequiredElement("form[data-resource-delete]").GetAttribute("action")!;
        using var content = new FormUrlEncodedContent(
            await PrepareForm(client, "/stellaradmin/Product")
        );

        // Act
        using var response = await client.PostAsync(action, content);

        // Assert
        await Assert.That(action).IsEqualTo("/stellaradmin/Product/Delete/2");
        if (rejected)
        {
            var result = await response.ReadDocumentAsync();
            await Assert
                .That(result.RequiredElement("form[data-resource-delete]").GetAttribute("action"))
                .IsEqualTo(action);
            await Assert
                .That(
                    result.RequiredElement("a[aria-label='Go to next page']").GetAttribute("href")
                )
                .IsEqualTo("/stellaradmin/Product?page=2");
        }
        else
        {
            await Assert
                .That(response.Headers.Location?.OriginalString)
                .IsEqualTo("/stellaradmin/Product");
        }
    }

    [Test]
    public async Task OutOfRangePageWithoutSelections_RedirectDoesNotAddDefaults()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(CreateState(), ConfigureSorting);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?page=9");

        // Assert
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=2");
    }

    private static void ConfigureSorting(ResourceBuilder<Product> resource) =>
        resource.Index(index =>
        {
            index.Columns(columns =>
            {
                columns.Clear();
                columns.Add(product => product.Name, column => column.Sortable = true);
                columns.Add(product => product.Price, column => column.Sortable = true);
                columns.Add(product => product.Id);
            });
            index.DefaultSortBy(product => product.Name);
            index.EnablePaging(paging =>
            {
                paging.PageSize = 2;
                paging.PageSizes = [2, 3];
            });
        });

    private static ProductState CreateState() =>
        new([
            new Product(4, "Charlie", 2),
            new Product(3, "Bravo", 3),
            new Product(2, "Alpha", 2),
            new Product(1, "Zulu", 1),
        ]);
}
