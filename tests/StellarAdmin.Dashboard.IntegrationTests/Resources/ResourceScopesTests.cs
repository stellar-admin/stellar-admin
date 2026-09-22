using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceScopesTests
{
    [Test]
    [Arguments("", "all", "1–2 of 5")]
    [Arguments("?scope=unknown", "all", "1–2 of 5")]
    [Arguments("?scope=UNDER-50", "under-50", "2 records")]
    [Arguments("?scope=50-and-over&search=lamp", "50-and-over", "2 records")]
    public async Task Scope_FiltersBeforeCountingAndPaging(
        string query,
        string expectedScope,
        string range
    )
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureScopes);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product" + query);

        // Assert
        await Assert.That(state.ListRequests.Single().Scope).IsEqualTo(expectedScope);
        await Assert
            .That(document.RequiredElement("[data-slot='data-grid-range']").TextContent.Trim())
            .IsEqualTo(range);
        await Assert
            .That(document.QuerySelectorAll("#index-page-scopes [aria-selected='true']").Length)
            .IsEqualTo(1);
        await Assert
            .That(
                document
                    .RequiredElement("#index-page-scopes [aria-selected='true']")
                    .GetAttribute("href")
            )
            .Contains(expectedScope == "all" ? "/stellaradmin/Product" : "scope=" + expectedScope);
    }

    [Test]
    public async Task DisabledScopes_IgnoreSelectionAndHideTabs()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product?scope=under-50");

        // Assert
        await Assert.That(state.ListRequests.Single().Scope).IsNull();
        await Assert.That(document.QuerySelector("#index-page-scopes")).IsNull();
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(5);
    }

    [Test]
    public async Task ScopeLinks_ResetPageAndPreserveExplicitChoices()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureScopes);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/Product?page=2&pageSize=2&sortBy=Name&sortDirection=desc&search=lamp"
        );
        var target = document
            .RequiredElement("#index-page-scopes a[href*='scope=under-50']")
            .GetAttribute("href")!;
        await client.GetDocumentAsync(target);

        // Assert
        await Assert
            .That(target)
            .IsEqualTo(
                "/stellaradmin/Product?scope=under-50&search=lamp&sortBy=Name&sortDirection=desc&pageSize=2"
            );
        await Assert.That(state.ListRequests.Last().Scope).IsEqualTo("under-50");
        await Assert.That(state.ListRequests.Last().Search).IsEqualTo("lamp");
        await Assert
            .That(state.ListRequests.Last().Sort)
            .IsEqualTo(new ResourceSort("Name", ResourceSortDirection.Descending));
        await Assert.That(state.ListRequests.Last().Paging).IsEqualTo(new ResourcePaging(1, 2));
    }

    [Test]
    public async Task IndexControls_PreserveScopeWithoutAddingDefaults()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureScopes);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/Product?scope=50-and-over&search=lamp"
        );
        var searchUrl = document.RequiredElement("input[name='search']").GetAttribute("hx-get")!;
        var cleared = await client.GetDocumentAsync(searchUrl + "&search=");

        // Assert
        await Assert.That(searchUrl).IsEqualTo("/stellaradmin/Product?scope=50-and-over");
        await Assert.That(state.ListRequests.Last().Scope).IsEqualTo("50-and-over");
        await Assert.That(state.ListRequests.Last().Search).IsNull();
        foreach (
            var link in cleared.QuerySelectorAll(
                "[data-slot='data-grid-pager'] a[href], [data-slot='data-grid-page-size'] a[href], [data-slot='data-grid-sort-link']"
            )
        )
        {
            await Assert.That(link.GetAttribute("href")).Contains("scope=50-and-over");
        }

        var all = document.RequiredElement("#index-page-scopes a");
        await Assert.That(all.GetAttribute("href")).IsEqualTo("/stellaradmin/Product?search=lamp");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Delete_PreservesScope(bool rejected)
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
                ConfigureScopes(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        const string url = "/stellaradmin/Product?scope=under-50";
        var document = await client.GetDocumentAsync(url);
        var action = document.RequiredElement("form[data-resource-delete]").GetAttribute("action")!;
        using var content = new FormUrlEncodedContent(await PrepareForm(client, url));

        // Act
        using var response = await client.PostAsync(action, content);

        // Assert
        await Assert.That(action).IsEqualTo("/stellaradmin/Product/Delete/1?scope=under-50");
        if (rejected)
        {
            var result = await response.ReadDocumentAsync();
            await Assert
                .That(
                    result
                        .RequiredElement("#index-page-scopes [aria-selected='true']")
                        .TextContent.Trim()
                )
                .IsEqualTo("Under 50");
            await Assert
                .That(result.RequiredElement(".validation-summary-errors").TextContent)
                .Contains("Still in use.");
            await Assert.That(state.ListRequests.Last().Scope).IsEqualTo("under-50");
        }
        else
        {
            await Assert
                .That(response.Headers.Location?.OriginalString)
                .IsEqualTo("/stellaradmin/Product?scope=under-50");
        }
    }

    [Test]
    public async Task OutOfRangePage_PreservesScopeWithoutAddingDefaults()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(CreateState(), ConfigureScopes);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/Product?page=9&scope=50-and-over"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=2&scope=50-and-over");
    }

    private static void ConfigureScopes(ResourceBuilder<Product> resource) =>
        resource.Index(index =>
        {
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All products");
                scopes.Add("under-50", "Under 50");
                scopes.Add("50-and-over", "50 and over");
                scopes.DefaultScope = "all";
            });
            index.EnableSearch();
            index.Columns(columns =>
                columns.Clear().Add(product => product.Name, column => column.Sortable())
            );
            index.DefaultSortBy(product => product.Name);
            index.EnablePaging(paging =>
            {
                paging.PageSize = 2;
                paging.PageSizes = [2, 3];
            });
        });

    private static ProductState CreateState() =>
        new([
            new Product(1, "Blue lamp", 10),
            new Product(2, "Desk", 100),
            new Product(3, "Green lamp", 60),
            new Product(4, "Red lamp", 70),
            new Product(5, "Yellow lamp", 20),
        ]);
}
