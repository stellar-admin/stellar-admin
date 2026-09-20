using System.Net;
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
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert.That(document.TextContents("thead th")).IsEquivalentTo(["Item"]);
        await Assert.That(document.TextContents("tbody td")).IsEquivalentTo(["Notebook"]);
    }

    [Test]
    [Arguments(false, "Browse Inventory", "Add Inventory item", "Save", "New Inventory item")]
    [Arguments(true, "Stock", "New stock item", "Add to stock", "Receive stock")]
    public async Task GlobalLabels_RespectsResourceOverrides(
        bool customize,
        string indexTitle,
        string createTitle,
        string submitLabel,
        string indexCreateLabel
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
            {
                resource.SingularLabel = "Inventory item";
                resource.PluralLabel = "Inventory";
                if (customize)
                {
                    resource.Index(index =>
                    {
                        index.Title = "Stock";
                        index.CreateLabel = "Receive stock";
                    });
                    resource.Create(create =>
                    {
                        create.Title = "New stock item";
                        create.SubmitLabel = "Add to stock";
                    });
                }
            },
            dashboard =>
                dashboard.ConfigureResourceLabels(labels =>
                {
                    labels.IndexTitle = resource => $"Browse {resource.PluralLabel}";
                    labels.IndexCreateLabel = resource => $"New {resource.SingularLabel}";
                    labels.CreateTitle = resource => $"Add {resource.SingularLabel}";
                    labels.CreateSubmitLabel = resource => "Save";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var index = await client.GetDocumentAsync("/stellaradmin/Product");
        var create = await client.GetDocumentAsync("/stellaradmin/Product/Create");

        // Assert
        await Assert
            .That(index.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo(indexTitle);
        await Assert
            .That(create.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo(createTitle);
        await Assert
            .That(
                index.RequiredElement("a[href='/stellaradmin/Product/Create']").TextContent.Trim()
            )
            .IsEqualTo(indexCreateLabel);
        await Assert
            .That(create.RequiredElement("button[type='submit']").TextContent.Trim())
            .IsEqualTo(submitLabel);
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
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
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
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo(expected);
    }
}
