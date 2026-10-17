using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Tabs;

public class TabsTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersTabsWebComponent()
    {
        // Arrange
        var sut = new TabsTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var tabs = html.QuerySelector("sel-tabs");
        await Assert.That(tabs?.GetAttribute("data-slot")).IsEqualTo("tabs-container");
        await Assert.That(tabs?.Id).StartsWith("--sa-tabs-");
        await Assert.That(tabs?.ClassList).Contains("sa-tabs-container");
        await Assert.That(tabs?.ClassList).Contains("existing-class");
    }

    [Test]
    public async Task ProcessAsync_WithId_KeepsAuthorIdAsCommandTarget()
    {
        // Arrange
        var sut = new TabsTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async context =>
                await TagHelperRenderer.RenderChildAsync(
                    new TabTagHelper { For = "summary" },
                    context
                ),
            [new TagHelperAttribute("id", "booking-tabs")]
        );

        // Assert
        await Assert.That(html.QuerySelector("sel-tabs")?.Id).IsEqualTo("booking-tabs");
        await Assert
            .That(html.QuerySelector("#summary-tab")?.GetAttribute("commandfor"))
            .IsEqualTo("booking-tabs");
    }

    [Test]
    public async Task ProcessAsync_WithoutValue_SelectsFirstTabAndPanel()
    {
        // Arrange
        var sut = new TabsTagHelper();

        // Act
        using var html = await RenderTabsAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("#summary-tab")?.GetAttribute("aria-selected"))
            .IsEqualTo("true");
        await Assert.That(html.QuerySelector("#summary")?.HasAttribute("hidden")).IsFalse();
        await Assert
            .That(html.QuerySelector("#itinerary-tab")?.GetAttribute("aria-selected"))
            .IsEqualTo("false");
        await Assert.That(html.QuerySelector("#itinerary")?.HasAttribute("hidden")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithValue_SelectsNamedTabAndPanel()
    {
        // Arrange
        var sut = new TabsTagHelper { Value = "itinerary" };

        // Act
        using var html = await RenderTabsAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("#summary-tab")?.GetAttribute("aria-selected"))
            .IsEqualTo("false");
        await Assert.That(html.QuerySelector("#summary")?.HasAttribute("hidden")).IsTrue();
        await Assert
            .That(html.QuerySelector("#itinerary-tab")?.GetAttribute("aria-selected"))
            .IsEqualTo("true");
        await Assert.That(html.QuerySelector("#itinerary")?.HasAttribute("hidden")).IsFalse();
    }

    private static Task<IHtmlDocument> RenderTabsAsync(TabsTagHelper sut) =>
        TagHelperRenderer.RenderAsync(
            sut,
            async context =>
                await TagHelperRenderer.RenderChildAsync(
                    new TabListTagHelper(),
                    context,
                    async list =>
                        await TagHelperRenderer.RenderChildAsync(
                            new TabTagHelper { For = "summary" },
                            list
                        )
                        + await TagHelperRenderer.RenderChildAsync(
                            new TabTagHelper { For = "itinerary" },
                            list
                        )
                )
                + await RenderPanelAsync(context, "summary")
                + await RenderPanelAsync(context, "itinerary")
        );

    private static Task<string> RenderPanelAsync(TagHelperContext context, string id) =>
        TagHelperRenderer.RenderChildAsync(
            new TabPanelTagHelper(),
            context,
            outputAttributes: [new TagHelperAttribute("id", id)]
        );
}
