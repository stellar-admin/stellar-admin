using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceSearchTests
{
    [Test]
    [Arguments("Apple", "2", "1–1 of 2")]
    [Arguments("   ", "1", "1–1 of 4")]
    [Arguments("Hidden", null, "0 records")]
    public async Task Search_FiltersInDatabaseBeforeCountingAndPaging(
        string term,
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
                    index.EnableSearch(
                        term => product => product.Name.Contains(term),
                        search => search.Placeholder = "Find a product"
                    );
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
        using var response = await client.GetAsync(
            "/stellaradmin/Product?search=" + Uri.EscapeDataString(term)
        );
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-slot='data-grid-range']")!.TextContent.Trim())
            .IsEqualTo(range);
        await Assert
            .That(document.QuerySelector("input[name='search']")!.GetAttribute("placeholder"))
            .IsEqualTo("Find a product");
        if (key is not null)
        {
            await Assert
                .That(document.QuerySelector("tbody td")!.TextContent.Trim())
                .IsEqualTo(key);
        }
        if (!string.IsNullOrWhiteSpace(term))
        {
            await Assert
                .That(
                    commands.Any(sql =>
                        sql.Contains("COUNT(*)") && sql.Contains("instr(") && !sql.Contains("LIMIT")
                    )
                )
                .IsTrue();
            await Assert
                .That(commands.Any(sql => sql.Contains("LIMIT") && sql.Contains("instr(")))
                .IsTrue();
        }
    }

    [Test]
    public async Task RepeatedSearch_ReplacesPredicateAndPlaceholderWithoutResettingIndex()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
        {
            resource.Index(index =>
                index.EnableSearch(
                    term => product => false,
                    search => search.Placeholder = "Old placeholder"
                )
            );
            resource.Index(index =>
                index.EnableSearch(term => product => product.Name.Contains(term))
            );
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?search=Apple");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(2);
        await Assert.That(document.QuerySelector("tbody td")!.TextContent.Trim()).IsEqualTo("2");
        await Assert
            .That(document.QuerySelector("input[name='search']")!.GetAttribute("placeholder"))
            .IsEqualTo("Search Products...");
    }

    [Test]
    public async Task BeyondSearchResults_RedirectsToFilteredLastPage()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
            {
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
        using var response = await client.GetAsync("/stellaradmin/Product?page=4&search=Apple");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=2&search=Apple");
    }
}
