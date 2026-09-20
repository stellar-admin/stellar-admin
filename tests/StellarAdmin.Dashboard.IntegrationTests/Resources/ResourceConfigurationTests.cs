using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceConfigurationTests
{
    [Test]
    public async Task AcrossRequests_ResolvesDataSourceFromEachRequestScope()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();

        // Act
        using var first = await client.GetAsync("/stellaradmin/Product");
        using var second = await client.GetAsync("/stellaradmin/Product");

        // Assert
        await Assert.That(first.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(second.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.Requests.Count).IsEqualTo(2);
        await Assert.That(state.Requests[0]).IsNotEqualTo(state.Requests[1]);
    }

    [Test]
    public async Task ClearedColumns_RendersOnlyReplacementColumns()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(1, "Notebook", 12.50m)]),
            resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                    {
                        columns.Clear();
                        columns.Add(product => product.Name).Title = "Item";
                    })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await new HtmlParser().ParseDocumentAsync(
            await client.GetStringAsync("/stellaradmin/Product")
        );

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("thead th")
                    .Select(element => element.TextContent.Trim())
                    .ToArray()
            )
            .IsEquivalentTo(["Item"]);
        await Assert
            .That(
                document
                    .QuerySelectorAll("tbody td")
                    .Select(element => element.TextContent.Trim())
                    .ToArray()
            )
            .IsEquivalentTo(["Notebook"]);
    }

    [Test]
    [Arguments(null, "Inventory")]
    [Arguments("Available products", "Available products")]
    public async Task LabelOverrides_RendersEffectiveTitleAtStableRoute(
        string? title,
        string expected
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
            {
                resource.PluralLabel = "Inventory";
                resource.Index(index => index.Title = title);
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var html = await client.GetStringAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(html);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='page-header-title']")?.TextContent.Trim())
            .IsEqualTo(expected);
    }

    [Test]
    [Arguments(null, "People")]
    [Arguments("Inventory", "Inventory")]
    public async Task RepeatedRegistration_RendersComposedLabels(string? plural, string expected)
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
            {
                dashboard.AddResource<Product>(resource =>
                {
                    resource.SingularLabel = "Item";
                    if (plural is not null)
                    {
                        resource.PluralLabel = plural;
                    }
                });
                dashboard.AddResource<Product>().SingularLabel = "Person";
                dashboard.AddResource<Product>();
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await new HtmlParser().ParseDocumentAsync(
            await client.GetStringAsync("/stellaradmin/Product")
        );

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='page-header-title']")?.TextContent.Trim())
            .IsEqualTo(expected);
    }
}
