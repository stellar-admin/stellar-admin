using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceCapabilitiesTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task UnregisteredActions_AreHidden(bool fullCrud)
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard => Configure(dashboard, fullCrud)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/InventoryItem");

        // Assert
        await Assert.That(document.Body!.TextContent).Contains("Read-only item");
        await Assert.That(document.QuerySelector("a[href*='/Create']")).IsNull();
        await Assert.That(document.QuerySelector("a[href*='/Edit/']")).IsNull();
        await Assert.That(document.QuerySelector("form[action*='/Delete/']")).IsNull();
    }

    [Test]
    [Arguments("Create", "", false)]
    [Arguments("Create", "", true)]
    [Arguments("Edit", "/SKU-123", false)]
    [Arguments("Edit", "/SKU-123", true)]
    [Arguments("Delete", "/SKU-123", false)]
    [Arguments("Delete", "/SKU-123", true)]
    public async Task UnregisteredAction_RejectsDirectRequests(
        string action,
        string key,
        bool fullCrud
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard => Configure(dashboard, fullCrud)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var get = await client.GetAsync($"/stellaradmin/InventoryItem/{action}{key}");
        using var post = await client.PostAsync(
            $"/stellaradmin/InventoryItem/{action}{key}",
            content
        );

        // Assert
        await Assert
            .That(get.StatusCode)
            .IsEqualTo(
                action == "Delete" ? HttpStatusCode.MethodNotAllowed : HttpStatusCode.NotFound
            );
        await Assert.That(post.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static void Configure(StellarAdminDashboardBuilder dashboard, bool fullCrud) =>
        dashboard.AddResource<InventoryItem>(resource =>
        {
            if (fullCrud)
            {
                dashboard.Services.AddSingleton(
                    new List<InventoryItem> { new("SKU-123") { Name = "Read-only item" } }
                );
                resource.UseDataSource<InventoryItemDataSource>();
            }
            else
            {
                resource.UseDataSource<ReadOnlyDataSource>();
            }
            resource.UseKey(item => item.Sku);
            resource.Index(index => index.Columns(columns => columns.Add(item => item.Name)));
        });

    public sealed class ReadOnlyDataSource : IResourceDataSource<InventoryItem>
    {
        public Task<IReadOnlyList<InventoryItem>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<InventoryItem>>([
                new("SKU-123") { Name = "Read-only item" },
            ]);
    }
}
