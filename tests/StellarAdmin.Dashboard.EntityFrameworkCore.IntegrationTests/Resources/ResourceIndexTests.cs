using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Options;
using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceIndexTests
{
    [Test]
    public async Task BeyondLastPage_RedirectsUsingFilteredTotal()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 2;
                    paging.PageSizes = [2, 4];
                })
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?page=3");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=2");
    }

    [Test]
    [Arguments("Create")]
    [Arguments("Edit/1")]
    public async Task DisabledForms_ReturnNotFound(string action)
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/Product/{action}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task EmptyDatabase_RendersEmptyState()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index => index.EnablePaging())
        );
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await db.Set<Product>().ExecuteDeleteAsync();
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(document.QuerySelector("[data-slot='empty-title']")).IsNotNull();
    }

    [Test]
    public async Task InvalidPaging_ReturnsBadRequest()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index => index.EnablePaging())
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?page=invalid");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task NoPaging_UsesMetadataKeyAndHonorsGlobalFilter()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.PluralLabel = "Catalog entries"
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("tbody tr td:first-child")
                    .Select(cell => cell.TextContent.Trim())
                    .ToArray()
            )
            .IsEquivalentTo(["1", "2", "3", "4"]);
        await Assert
            .That(document.QuerySelector("h1")!.TextContent.Trim())
            .IsEqualTo("Catalog entries");
        await Assert.That(document.QuerySelector("a[href$='/Create']")).IsNull();
        var options = sut.Services.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        await Assert.That(options.KeySelector!(new Product { Number = 42 })).IsEqualTo("42");
    }

    [Test]
    [Arguments("", "3,4")]
    [Arguments("&sortBy=Name&sortDirection=asc", "4,1")]
    [Arguments("&sortBy=Name&sortDirection=desc", "2,3")]
    [Arguments("&sortBy=Price&sortDirection=desc", "2,1")]
    public async Task PagingAndSorting_ExecuteInDatabaseWithStableOrdering(
        string sort,
        string expected
    )
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                    index.EnablePaging(paging =>
                    {
                        paging.PageSize = 2;
                        paging.PageSizes = [2, 4];
                    })
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/Product?page=2{sort}");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                string.Join(
                    ',',
                    document
                        .QuerySelectorAll("tbody tr td:first-child")
                        .Select(cell => cell.TextContent.Trim())
                )
            )
            .IsEqualTo(expected);
        await Assert
            .That(
                commands.Any(sql =>
                    sql.Contains("LIMIT") && sql.Contains("OFFSET") && sql.Contains("ORDER BY")
                )
            )
            .IsTrue();
        await Assert
            .That(commands.Any(sql => sql.Contains("COUNT(*)") && !sql.Contains("LIMIT")))
            .IsTrue();
    }

    [Test]
    public async Task UnknownSort_UsesConfiguredDefaultDescending()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index => index.DefaultSortByDescending(product => product.Price))
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?sortBy=Hidden");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                string.Join(
                    ',',
                    document
                        .QuerySelectorAll("tbody tr td:first-child")
                        .Select(cell => cell.TextContent.Trim())
                )
            )
            .IsEqualTo("4,3,2,1");
    }
}
