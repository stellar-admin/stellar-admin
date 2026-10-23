using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

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

    [Test]
    public async Task ProjectedPanel_RowDetailRendersProjection()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
                index.EnableRowDetails(panels =>
                    panels.Add(
                        "Pricing",
                        "_ProductPricing",
                        product => new ProductPricing(
                            product.Name,
                            product.Price,
                            product.Category != null ? product.Category.Name : null
                        )
                    )
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/rowdetail/2?panel=pricing"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("<p data-test=\"pricing\">Apple, 20, Technology</p>");
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
