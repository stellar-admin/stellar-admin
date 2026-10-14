using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Tabs;

public class TabTagHelperTests
{
    [Test]
    public async Task ProcessAsync_InTabs_RendersTabButtonForPanel()
    {
        // Arrange
        var sut = new TabTagHelper { For = "summary" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper(),
            async context =>
                await TagHelperRenderer.RenderChildAsync(
                    sut,
                    context,
                    _ => Task.FromResult("Summary")
                )
        );

        // Assert
        var tab = html.QuerySelector("#summary-tab");
        await Assert.That(tab?.TagName).IsEqualTo("BUTTON");
        await Assert.That(tab?.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(tab?.GetAttribute("role")).IsEqualTo("tab");
        await Assert.That(tab?.GetAttribute("command")).IsEqualTo("--select-tab");
        await Assert
            .That(tab?.GetAttribute("commandfor"))
            .IsEqualTo(html.QuerySelector("sel-tabs")?.Id);
        await Assert.That(tab?.GetAttribute("aria-controls")).IsEqualTo("summary");
        await Assert.That(tab?.GetAttribute("data-slot")).IsEqualTo("tabs-trigger");
        await Assert.That(tab?.ClassList).Contains("sa-tabs-trigger");
        await Assert.That(tab?.ClassList).Contains("existing-class");
        await Assert.That(tab?.TextContent).IsEqualTo("Summary");
    }

    [Test]
    public async Task ProcessAsync_WhenSelected_RendersSelectedFocusableTab()
    {
        // Arrange
        var sut = new TabTagHelper { For = "summary" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper { Selected = "summary" },
            async context => await TagHelperRenderer.RenderChildAsync(sut, context)
        );

        // Assert
        var tab = html.QuerySelector("#summary-tab");
        await Assert.That(tab?.GetAttribute("aria-selected")).IsEqualTo("true");
        await Assert.That(tab?.GetAttribute("tabindex")).IsEqualTo("0");
        await Assert.That(tab?.HasAttribute("data-active")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WhenNotSelected_RendersUnselectedTabOutOfTabOrder()
    {
        // Arrange
        var sut = new TabTagHelper { For = "summary" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper { Selected = "itinerary" },
            async context => await TagHelperRenderer.RenderChildAsync(sut, context)
        );

        // Assert
        var tab = html.QuerySelector("#summary-tab");
        await Assert.That(tab?.GetAttribute("aria-selected")).IsEqualTo("false");
        await Assert.That(tab?.GetAttribute("tabindex")).IsEqualTo("-1");
        await Assert.That(tab?.HasAttribute("data-active")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenDisabled_DisablesButton()
    {
        // Arrange
        var sut = new TabTagHelper { For = "summary", IsDisabled = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper(),
            async context => await TagHelperRenderer.RenderChildAsync(sut, context)
        );

        // Assert
        await Assert.That(html.QuerySelector("#summary-tab")?.HasAttribute("disabled")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithId_KeepsAuthorId()
    {
        // Arrange
        var sut = new TabTagHelper { For = "summary" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper(),
            async context =>
                await TagHelperRenderer.RenderChildAsync(
                    sut,
                    context,
                    outputAttributes: [new TagHelperAttribute("id", "summary-trigger")]
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelector("#summary-trigger")?.GetAttribute("aria-controls"))
            .IsEqualTo("summary");
    }

    [Test]
    public async Task ProcessAsync_OutsideTabs_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new TabTagHelper { For = "summary" };

        // Act
        var act = async () => await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WithoutFor_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new TabTagHelper();

        // Act
        var act = async () =>
            await TagHelperRenderer.RenderAsync(
                new TabsTagHelper(),
                async context => await TagHelperRenderer.RenderChildAsync(sut, context)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
