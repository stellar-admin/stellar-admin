using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Scenarios.Tests.Infrastructure;

namespace StellarAdmin.Scenarios.Tests.Branding;

// Assertions a spec file cannot express stay in C#; this one checks the server-rendered HTML
public class BrandingScenarioTests
{
    private const string Header = "[data-slot='sidebar-header'] [data-slot='sidebar-menu-button']";

    [Test]
    public async Task Http_SidebarHeaderRendersTitleSubtitleAndIcon()
    {
        // Arrange
        await using var app = await ScenarioApp.StartTestServerAsync(new BrandingScenario());
        using var client = app.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var header = document.QuerySelector(Header);
        await Assert.That(header).IsNotNull();
        await Assert
            .That(header!.QuerySelector(".sa-sidebar-brand-title")?.TextContent)
            .IsEqualTo(BrandingScenario.Title);
        await Assert
            .That(header.QuerySelector(".sa-sidebar-brand-subtitle")?.TextContent)
            .IsEqualTo(BrandingScenario.Subtitle);
        await Assert.That(header.QuerySelector(".sa-sidebar-brand-mark > svg")).IsNotNull();
    }
}
