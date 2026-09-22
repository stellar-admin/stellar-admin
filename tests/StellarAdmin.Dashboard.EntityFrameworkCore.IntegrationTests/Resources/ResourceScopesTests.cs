using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceScopesTests
{
    [Test]
    [Arguments("", "1", "1–1 of 2")]
    [Arguments("?scope=unknown", "1", "1–1 of 2")]
    [Arguments("?scope=AFFORDABLE&search=Apple", "2", "1 record")]
    [Arguments("?scope=all", "1", "1–1 of 4")]
    [Arguments("?scope=expensive&page=2", "4", "2–2 of 2")]
    [Arguments("?scope=expensive&search=Hidden", null, "0 records")]
    public async Task Scope_FiltersWithSearchBeforeCountingAndPaging(
        string query,
        string? key,
        string range
    )
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                {
                    index.EnableScopes(scopes =>
                    {
                        scopes.Add("all", "All products");
                        scopes.Add("affordable", "Affordable", product => product.Price < 30);
                        scopes.Add("expensive", "Expensive", product => product.Price >= 30);
                        scopes.DefaultScope = "affordable";
                    });
                    index.EnableSearch(term => product => product.Name.Contains(term));
                    index.EnablePaging(paging =>
                    {
                        paging.PageSize = 1;
                        paging.PageSizes = [1, 2];
                    });
                }),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product" + query);
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-slot='data-grid-range']")!.TextContent.Trim())
            .IsEqualTo(range);
        await Assert
            .That(document.QuerySelectorAll("#index-page-scopes [aria-selected='true']").Length)
            .IsEqualTo(1);
        if (key is not null)
        {
            await Assert
                .That(document.QuerySelector("tbody td")!.TextContent.Trim())
                .IsEqualTo(key);
        }
        if (query != "?scope=all")
        {
            await Assert
                .That(
                    commands.Any(sql =>
                        sql.Contains("COUNT(*)")
                        && sql.Contains("\"Price\"")
                        && !sql.Contains("LIMIT")
                    )
                )
                .IsTrue();
            await Assert
                .That(
                    commands.Any(sql =>
                        sql.Contains("LIMIT") && (sql.Contains("< 30") || sql.Contains(">= 30"))
                    )
                )
                .IsTrue();
        }
    }

    [Test]
    public async Task RepeatedScopes_ReplacesEntriesPredicatesAndDefault()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
        {
            resource.Index(index =>
                index.EnableScopes(scopes =>
                {
                    scopes.Add("old", "Old scope", product => false);
                    scopes.DefaultScope = "old";
                })
            );
            resource.Index(index => index.EnableScopes().Add("all", "All products"));
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?scope=old");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(4);
        await Assert.That(document.QuerySelectorAll("#index-page-scopes a").Length).IsEqualTo(1);
        await Assert
            .That(document.QuerySelector("#index-page-scopes")!.TextContent)
            .Contains("All products");
    }

    [Test]
    public async Task BeyondScopedResults_RedirectsToFilteredLastPage()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
            {
                index.EnableScopes(scopes =>
                    scopes.Add("expensive", "Expensive", product => product.Price >= 30)
                );
                index.EnableSearch(term => product => product.Name.Contains(term));
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 1;
                    paging.PageSizes = [1, 2];
                });
            })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/Product?scope=expensive&search=Apple&page=4"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=1&search=Apple&scope=expensive");
    }
}
