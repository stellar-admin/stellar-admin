using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Displays;
using TUnit.Assertions.Enums;

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

    [Test]
    public async Task View_WithCollectionField_LoadsTheCollection()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowView(view =>
                view.Fields(fields =>
                    fields
                        .Add(product => product.Tags)
                        .UseDisplay<BadgeDisplay>(badge => badge.UseText<Tag>(tag => tag.Name))
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/view/2");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(document.QuerySelectorAll("[data-slot='badge']").Length).IsEqualTo(2);
    }

    [Test]
    public async Task View_WithCollectionFieldWithoutText_LogsProblem()
    {
        // Arrange

        // Act
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowView(view =>
                view.Fields(fields =>
                    fields.Add(product => product.Tags).UseDisplay<BadgeDisplay>()
                )
            )
        );

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "The view page's field 'Tags' shows a collection of Tag, which has no text of its own."
                    )
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task View_WithCollectionFieldText_ShowsTheTextOfEachItem()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowView(view =>
                view.Fields(fields =>
                    fields
                        .Add(product => product.Tags)
                        .UseDisplay<BadgeDisplay>(badge => badge.UseText<Tag>(tag => tag.Name))
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/view/2");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='badge']")
                    .Select(badge => badge.TextContent.Trim())
            )
            .IsEquivalentTo(["Sale", "Seasonal"], CollectionOrdering.Any);
    }

    [Test]
    public async Task View_WithComputedFields_LoadsTheNavigationsTheyRead()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(
                        product =>
                            string.Join(
                                ", ",
                                product.Tags.OrderBy(tag => tag.Name).Select(tag => tag.Name)
                            ),
                        field => field.Title = "Tags"
                    );
                    fields.Add(
                        product => product.Category!.Name + " / " + product.Name,
                        field => field.Title = "Category"
                    );
                })
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/view/2");
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='display-field-value']")
                    .Select(value => value.TextContent.Trim())
            )
            .IsEquivalentTo(["Sale, Seasonal", "Technology / Apple"], CollectionOrdering.Matching);
    }
}
