using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Testing;
using TUnit.Assertions.Enums;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceLayoutsTests
{
    [Test]
    [Arguments("?layout=totals", new[] { "Office: 1", "Technology: 1" })]
    [Arguments("?layout=totals&scope=all&search=Apple", new[] { "Technology: 2" })]
    public async Task EfCoreHandler_ReshapesTheEntitiesTheScopeAndSearchSelect(
        string query,
        string[] totals
    )
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(
                index =>
                {
                    index.EnableScopes(scopes =>
                    {
                        scopes.Add("all", "All products");
                        scopes.Add("affordable", "Affordable", product => product.Price < 30);
                        scopes.DefaultScope = "affordable";
                    });
                    index.EnableSearch(term => product => product.Name.Contains(term));
                },
                layouts =>
                {
                    layouts.AddTable("products");
                    layouts.AddPartial<CategoryTotal, CategoryTotalsHandler>(
                        "totals",
                        "_CategoryTotals"
                    );
                }
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products" + query);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("#index-page-data-grid [data-test='total']")
                    .Select(total => total.TextContent.Trim())
            )
            .IsEquivalentTo(totals, CollectionOrdering.Matching);
    }

    [Test]
    public async Task Cards_LoadTheirValuesThroughNavigationsInTheirDefaultSort()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(
                _ => { },
                layouts =>
                    layouts.AddCards(
                        "cards",
                        cards =>
                        {
                            cards.DefaultSortByDescending(product => product.Price);
                            cards.Card(card =>
                            {
                                card.Badge(product => product.Category!.Name);
                                card.Fields(fields =>
                                    fields
                                        .Add(product => product.Tags)
                                        .UseDisplay<BadgeDisplay>(badge =>
                                            badge.UseText<Tag>(tag => tag.Name)
                                        )
                                );
                            });
                        }
                    )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll(".sa-resource-card")
                    .Select(card =>
                        $"{card.QuerySelector(".sa-card-title > :first-child")!.TextContent.Trim()}: "
                        + $"{card.QuerySelector(".sa-resource-card-badge")?.TextContent.Trim()}: "
                        + card.QuerySelector(".sa-resource-card-fields dd")!.GetAttribute("title")
                    )
            )
            .IsEquivalentTo(
                [
                    "Banana: Beverage: —",
                    "Apple: Technology: —",
                    "Apple: Technology: Sale Seasonal",
                    "Zebra: Office: —",
                ],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task EfCoreHandlerTable_ShowsTheReshapedEntitiesInItsColumns()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(
                index => index.EnableSearch(term => product => product.Name.Contains(term)),
                layouts =>
                {
                    layouts.AddTable("products");
                    layouts.AddTable<CategoryTotal, CategoryTotalsHandler>(
                        "totals",
                        totals =>
                            totals.Columns(columns =>
                            {
                                columns.Add(total => total.Category);
                                columns.Add(total => total.Products);
                            })
                    );
                }
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products?layout=totals&search=Apple"
        );
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("#index-page-grid tbody tr")
                    .Select(row =>
                        string.Join(
                            " / ",
                            row.QuerySelectorAll("td").Select(cell => cell.TextContent.Trim())
                        )
                    )
            )
            .IsEquivalentTo(["Technology / 2"]);
    }

    [Test]
    public async Task EfCoreHandlerCards_ShowTheReshapedEntities()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(
                index => index.EnableSearch(term => product => product.Name.Contains(term)),
                layouts =>
                {
                    layouts.AddTable("products");
                    layouts.AddCards<CategoryTotal, CategoryTotalsHandler>(
                        "totals",
                        totals =>
                            totals.Card(card =>
                            {
                                card.Title(total => total.Category);
                                card.Stats(stats => stats.Add(total => total.Products));
                            })
                    );
                }
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products?layout=totals&search=Apple"
        );
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll(".sa-resource-card")
                    .Select(card =>
                        card.QuerySelector(".sa-card-title")!.TextContent.Trim()
                        + " / "
                        + card.QuerySelector(".sa-resource-card-stats dd")!.TextContent.Trim()
                    )
            )
            .IsEquivalentTo(["Technology / 2"]);
    }

    [Test]
    public async Task ResourceHandler_LoadsItsOwnData()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(
                _ => { },
                layouts =>
                {
                    layouts.AddTable("products");
                    layouts.AddPartial<CategoryTotal, CategoryNamesHandler>(
                        "categories",
                        "_CategoryTotals"
                    );
                }
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products?layout=categories");
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-test='total']")
                    .Select(total => total.TextContent.Trim())
            )
            .IsEquivalentTo(
                ["Beverage: 0", "Office: 0", "Technology: 0"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task HandlerOfNeitherKind_Throws()
    {
        // Arrange

        // Act
        Func<Task> act = async () =>
        {
            await using var _ = await EfCoreTestHost.CreateAsync(resource =>
                resource.Index(
                    _ => { },
                    layouts =>
                        layouts.AddPartial<CategoryTotal, CategoryTotal>(
                            "totals",
                            "_CategoryTotals"
                        )
                )
            );
        };

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        await Assert.That(exception!.Message).Contains("CategoryTotal");
    }
}
