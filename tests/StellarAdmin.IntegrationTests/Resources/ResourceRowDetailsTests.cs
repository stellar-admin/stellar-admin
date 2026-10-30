using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.IntegrationTests.Resources;

// The row detail endpoint's answers to URLs the index page never requests; what people see is covered by
// the row details specs in StellarAdmin.ScenarioTesting.Tests
public class ResourceRowDetailsTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task DetailsNotLoadedOnDemand_RowDetailReturnsNotFound(bool inlineDetails)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                if (inlineDetails)
                {
                    resource.Index(index => index.EnableRowDetails("_ProductSummary"));
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/rowdetail/7");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(await response.Content.ReadAsStringAsync()).IsEmpty();
    }

    [Test]
    [Arguments("")]
    [Arguments("?panel=summary")]
    [Arguments("?panel=missing")]
    public async Task Panels_RowDetailWithoutOnDemandPanel_ReturnsNotFound(string query)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePanels
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/rowdetail/7{query}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(await response.Content.ReadAsStringAsync()).IsEmpty();
    }

    private static void ConfigurePanels(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.Index(index =>
            index.EnableRowDetails(panels =>
            {
                panels.Add<ProductNotesHandler>("Notes", "_ProductNotes");
                panels.Add("Summary", "_ProductSummary");
            })
        );
    }
}
