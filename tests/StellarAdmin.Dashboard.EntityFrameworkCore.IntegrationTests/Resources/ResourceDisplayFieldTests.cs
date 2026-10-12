using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceDisplayFieldTests
{
    [Test]
    public async Task Edit_WithDisplayFieldsThroughReferences_LoadsReferencesWithEntity()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Price);
                        fields.AddDisplayField(product => product.Category!.Name);
                        fields.AddDetails(
                            "Details",
                            details => details.Add(product => product.Details.Sku)
                        );
                    })
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");
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
    public async Task Edit_WithDisplayFieldOnOwnProperty_DoesNotJoinReferences()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Price);
                        fields.AddDisplayField(product => product.CategoryId);
                    })
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(commands.Single()).DoesNotContain("JOIN \"Category\"");
    }
}
