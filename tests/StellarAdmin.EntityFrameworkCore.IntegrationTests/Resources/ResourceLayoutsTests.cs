using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
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
