using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Testing;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceSidebarTests
{
    [Test]
    public async Task EfCoreResource_RendersSidebarOverride()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
        {
            resource.PluralLabel = "Catalog entries";
            resource.SidebarItem(item => item.Group = "Commerce");
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='sidebar-group-label']")?.TextContent.Trim())
            .IsEqualTo("Commerce");
        await Assert
            .That(
                document
                    .QuerySelector("[data-slot='sidebar-group'] [data-slot='sidebar-menu-button']")
                    ?.TextContent.Trim()
            )
            .IsEqualTo("Catalog entries");
        await Assert
            .That(
                document
                    .QuerySelector("[data-slot='sidebar-group'] [data-slot='sidebar-menu-button']")
                    ?.GetAttribute("href")
            )
            .IsEqualTo("/stellaradmin/products");
    }
}
