using Microsoft.AspNetCore.TestHost;
using Microsoft.Playwright;
using StellarAdmin.Scenarios.Tests.Infrastructure;

namespace StellarAdmin.Scenarios.Tests.Branding;

public class BrandingScenarioTests
{
    private const string Header = "[data-slot='sidebar-header'] [data-slot='sidebar-menu-button']";

    [ClassDataSource<BrowserFixture>(Shared = SharedType.PerTestSession)]
    public required BrowserFixture Browser { get; init; }

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

    [Test]
    public async Task Browser_SidebarHeaderShowsTitleSubtitleAndIcon()
    {
        // Arrange
        var (app, baseUrl) = await ScenarioApp.StartKestrelAsync(new BrandingScenario());
        await using var _ = app;
        var page = await Browser.NewPageAsync(baseUrl);

        // Act
        await page.GotoAsync("/stellaradmin");

        // Assert
        var header = page.Locator(Header);
        await Assertions
            .Expect(header.Locator(".sa-sidebar-brand-title"))
            .ToHaveTextAsync(BrandingScenario.Title);
        await Assertions
            .Expect(header.Locator(".sa-sidebar-brand-subtitle"))
            .ToHaveTextAsync(BrandingScenario.Subtitle);
        var icon = await header.Locator(".sa-sidebar-brand-mark > svg").BoundingBoxAsync();
        await Assert.That(icon).IsNotNull();
        await Assert.That(icon!.Width).IsGreaterThan(0);
    }
}
