using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Displays;
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

    [Test]
    public async Task Index_WithCollectionColumn_LoadsTheCollectionForEachRowOfThePage()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                    columns.Add(
                        product => product.Tags,
                        column => column.UseDisplay<BadgeDisplay>()
                    )
                );
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 2;
                    paging.PageSizes = [2];
                });
            })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var rows = document.QuerySelectorAll("tbody tr");
        await Assert
            .That(rows.Select(row => row.QuerySelector("td")!.TextContent.Trim()))
            .IsEquivalentTo(["1", "2"], CollectionOrdering.Matching);
        await Assert
            .That(rows.Select(row => row.QuerySelectorAll("[data-slot='badge']").Length))
            .IsEquivalentTo([0, 2], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Index_WithSortableCollectionColumn_FailsToStart()
    {
        // Arrange

        // Act
        async Task act()
        {
            await using var sut = await EfCoreTestHost.CreateAsync(resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                        columns.Add(product => product.Tags, column => column.Sortable())
                    )
                )
            );
        }

        // Assert
        var exception = await Assert.That(act).Throws<InvalidOperationException>();
        await Assert.That(exception!.Message).Contains("can't be sortable");
    }

    [Test]
    public async Task Index_WithCollectionColumnText_ShowsTheTextOfEachItem()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
                index.Columns(columns =>
                    columns.Add(
                        product => product.Tags,
                        column =>
                            column.UseDisplay<BadgeDisplay>(badge =>
                                badge.UseText<Tag>(tag => tag.Name)
                            )
                    )
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("tbody [data-slot='badge']")
                    .Select(badge => badge.TextContent.Trim())
            )
            .IsEquivalentTo(["Sale", "Seasonal"], CollectionOrdering.Any);
    }

    [Test]
    public async Task Index_WithCollectionColumnTextOfAnotherType_FailsToStart()
    {
        // Arrange

        // Act
        async Task act()
        {
            await using var sut = await EfCoreTestHost.CreateAsync(resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                        columns.Add(
                            product => product.Tags,
                            column =>
                                column.UseDisplay<BadgeDisplay>(badge =>
                                    badge.UseText<Product>(product => product.Name)
                                )
                        )
                    )
                )
            );
        }

        // Assert
        var exception = await Assert.That(act).Throws<OptionsValidationException>();
        await Assert
            .That(exception!.Message)
            .Contains("The index column 'Tags' shows Tag badges, but its UseText reads Product.");
    }
}
