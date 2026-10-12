using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceRowDetailsTests
{
    [Test]
    public async Task ProjectedDetails_RenderLoaderForTheEntityKey()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureProjectedDetails);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(html).Contains("hx-get=\"/stellaradmin/products/rowdetail/2\"");
        await Assert.That(html).DoesNotContain("data-test=\"pricing\"");
    }

    [Test]
    [Arguments("2", "Apple, 20, Technology")]
    [Arguments("1", "Zebra, 10, Office")]
    public async Task ProjectedDetails_RowDetailRendersProjection(string id, string expected)
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureProjectedDetails);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/rowdetail/{id}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains($"<p data-test=\"pricing\">{expected}</p>");
    }

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

    private static void ConfigureProjectedDetails(
        EfCoreResourceBuilder<CatalogDbContext, Product> resource
    ) =>
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
