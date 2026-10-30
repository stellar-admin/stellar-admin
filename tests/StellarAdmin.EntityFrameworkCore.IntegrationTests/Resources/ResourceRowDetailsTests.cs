using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

// The row detail endpoint's answers to keys the index page never requests; what people see is covered by
// the row details specs in StellarAdmin.ScenarioTesting.Tests
public class ResourceRowDetailsTests
{
    [Test]
    [Arguments("99")]
    [Arguments("not-a-number")]
    [Arguments("5")]
    public async Task ProjectedDetails_MissingOrFilteredEntity_ReturnsNotFound(string id)
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureProjectedDetails);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/rowdetail/{id}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static void ConfigureProjectedDetails(EfCoreResourceBuilder<Product> resource) =>
        resource.Index(index =>
            index.EnableRowDetails(
                "_ProductPricing",
                product => new ProductPricing(
                    product.Name,
                    product.Price,
                    product.Category != null ? product.Category.Name : null
                )
            )
        );
}
