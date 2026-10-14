using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using TUnit.Assertions.Enums;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceColumnTests
{
    [Test]
    public async Task Index_WithColumnThroughReference_LoadsReferenceWithPage()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                        columns.Add(product => product.Category!.Name, column => column.Sortable())
                    )
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products?sortBy=Category.Name&sortDirection=desc"
        );
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("tbody tr td:last-child")
                    .Select(element => element.TextContent.Trim())
            )
            .IsEquivalentTo(
                ["Technology", "Technology", "Office", "Beverage"],
                CollectionOrdering.Matching
            );
        await Assert.That(commands.Last()).Contains("JOIN \"Category\"");
    }

    [Test]
    public async Task Index_WithColumnsOnOwnProperties_DoesNotJoinReferences()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                    {
                        columns.Add(product => product.CategoryId);
                        columns.Add(product => product.Details.Sku);
                    })
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(commands.Any(command => command.Contains("JOIN"))).IsFalse();
    }
}
