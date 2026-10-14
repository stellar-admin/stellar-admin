using AngleSharp.Dom;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceSidebarTests
{
    [Test]
    public async Task ConfiguredItems_RenderLinksInGroupsAndOrder()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource => resource.SidebarItem(item => item.Order = 10),
            admin =>
            {
                admin.AddResource<CustomProduct>(
                    "catalog",
                    resource =>
                    {
                        resource.UseDataSource<CustomProductDataSource>();
                        resource.SidebarItem(item =>
                        {
                            item.Label = "Custom catalog";
                            item.Order = -1;
                        });
                    }
                );
                admin.AddResource<Product>(resource => resource.PluralLabel = "Inventory");
                admin.AddResource<InventoryItem>(resource =>
                    resource.SidebarItem(item =>
                    {
                        item.Group = "Hidden";
                        item.Visible = false;
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");
        var links = document
            .QuerySelectorAll("[data-slot='sidebar-group'] [data-slot='sidebar-menu-button']")
            .ToArray();

        // Assert
        await Assert
            .That(document.TextContents("[data-slot='sidebar-group-label']"))
            .IsEquivalentTo(["Resources"]);
        await Assert.That(links.Length).IsEqualTo(2);
        await Assert.That(links[0].TextContent.Trim()).IsEqualTo("Custom catalog");
        await Assert.That(links[0].GetAttribute("href")).IsEqualTo("/stellaradmin/catalog");
        await Assert.That(links[1].TextContent.Trim()).IsEqualTo("Inventory");
        await Assert.That(links[1].GetAttribute("href")).IsEqualTo("/stellaradmin/products");
    }
}
