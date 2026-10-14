using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceViewTests
{
    [Test]
    public async Task View_WithFieldThroughReference_LoadsReferenceWithEntity()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowView(view =>
                    view.Fields(fields =>
                        fields.AddSection(
                            "Details",
                            section =>
                            {
                                section.Add(product => product.Category!.Name);
                                section.Add(product => product.Details.Sku);
                            }
                        )
                    )
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/view/1");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='display-field-value']")
                    .Select(element => element.TextContent.Trim())
            )
            .IsEquivalentTo(["Office", "Z-1"]);
        await Assert.That(commands.Count).IsEqualTo(1);
        await Assert.That(commands.Single()).Contains("JOIN \"Category\"");
    }

    [Test]
    public async Task View_WithFieldsOnOwnProperties_DoesNotJoinReferences()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowView(view =>
                    view.Fields(fields => fields.Add(product => product.CategoryId))
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/view/1");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(commands.Single()).DoesNotContain("JOIN \"Category\"");
    }
}
