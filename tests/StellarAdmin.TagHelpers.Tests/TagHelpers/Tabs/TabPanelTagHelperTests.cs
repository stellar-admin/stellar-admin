using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Tabs;

public class TabPanelTagHelperTests
{
    [Test]
    public async Task ProcessAsync_InTabs_RendersPanelLabelledByItsTab()
    {
        // Arrange
        var sut = new TabPanelTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper(),
            async context =>
                await TagHelperRenderer.RenderChildAsync(
                    new TabTagHelper { For = "summary" },
                    context
                ) + await RenderPanelAsync(sut, context, "summary")
        );

        // Assert
        var panel = html.QuerySelector("#summary");
        await Assert.That(panel?.TagName).IsEqualTo("DIV");
        await Assert.That(panel?.GetAttribute("role")).IsEqualTo("tabpanel");
        await Assert.That(panel?.GetAttribute("aria-labelledby")).IsEqualTo("summary-tab");
        await Assert.That(panel?.GetAttribute("tabindex")).IsEqualTo("0");
        await Assert.That(panel?.GetAttribute("data-slot")).IsEqualTo("tabs-content");
        await Assert.That(panel?.HasAttribute("hidden")).IsFalse();
        await Assert.That(panel?.ClassList).Contains("sa-tab-panel");
        await Assert.That(panel?.ClassList).Contains("existing-class");
    }

    [Test]
    public async Task ProcessAsync_WithTabId_LabelsPanelWithTabId()
    {
        // Arrange
        var sut = new TabPanelTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper(),
            async context =>
                await TagHelperRenderer.RenderChildAsync(
                    new TabTagHelper { For = "summary" },
                    context,
                    outputAttributes: [new TagHelperAttribute("id", "summary-trigger")]
                ) + await RenderPanelAsync(sut, context, "summary")
        );

        // Assert
        await Assert
            .That(html.QuerySelector("#summary")?.GetAttribute("aria-labelledby"))
            .IsEqualTo("summary-trigger");
    }

    [Test]
    public async Task ProcessAsync_WhenNotSelected_RendersHiddenPanel()
    {
        // Arrange
        var sut = new TabPanelTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper { Selected = "itinerary" },
            async context => await RenderPanelAsync(sut, context, "summary")
        );

        // Assert
        await Assert.That(html.QuerySelector("#summary")?.HasAttribute("hidden")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithLinkTabListInside_RendersNoTabListRole()
    {
        // Arrange
        var sut = new TabPanelTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper(),
            async context =>
                await RenderPanelAsync(
                    sut,
                    context,
                    "summary",
                    async panel =>
                        await TagHelperRenderer.RenderChildAsync(new TabListTagHelper(), panel)
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelector("#summary [data-slot=tabs-list]")?.HasAttribute("role"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_OutsideTabs_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new TabPanelTagHelper();

        // Act
        var act = async () =>
            await TagHelperRenderer.RenderAsync(
                sut,
                outputAttributes: [new TagHelperAttribute("id", "summary")]
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WithoutId_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new TabPanelTagHelper();

        // Act
        var act = async () =>
            await TagHelperRenderer.RenderAsync(
                new TabsTagHelper(),
                async context => await TagHelperRenderer.RenderChildAsync(sut, context)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    private static Task<string> RenderPanelAsync(
        TabPanelTagHelper sut,
        TagHelperContext context,
        string id,
        Func<TagHelperContext, Task<string>>? children = null
    ) =>
        TagHelperRenderer.RenderChildAsync(
            sut,
            context,
            children,
            [new TagHelperAttribute("id", id)]
        );
}
