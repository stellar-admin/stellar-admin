using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Tabs;

public class TabListTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithLinkTabs_RendersNoTabListRole()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var list = html.QuerySelector("[data-slot=tabs-list]");
        await Assert.That(list?.HasAttribute("role")).IsFalse();
        await Assert.That(list?.HasAttribute("aria-orientation")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_InTabs_RendersTabListWithOrientation()
    {
        // Arrange
        var sut = CreateSut();
        sut.Orientation = TabListOrientation.Vertical;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new TabsTagHelper(),
            async context => await TagHelperRenderer.RenderChildAsync(sut, context)
        );

        // Assert
        var list = html.QuerySelector("[data-slot=tabs-list]");
        await Assert.That(list?.GetAttribute("role")).IsEqualTo("tablist");
        await Assert.That(list?.GetAttribute("aria-orientation")).IsEqualTo("vertical");
    }

    [Test]
    public async Task ProcessAsync_ByDefault_RendersHiddenMoreTriggerForMenu()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var host = html.QuerySelector("sel-tab-list.sa-tabs");
        var trigger = html.QuerySelector("[data-slot=tabs-list] > [data-slot=tabs-more]");
        var menuId = trigger?.GetAttribute("popovertarget");
        var menu = html.QuerySelector("sel-tab-list > sel-dropdown-menu[popover][role=menu]");
        await Assert.That(host).IsNotNull();
        await Assert.That(trigger?.HasAttribute("hidden")).IsTrue();
        await Assert.That(trigger?.TextContent).IsEqualTo("More");
        await Assert.That(menuId).IsNotNull().And.IsNotEmpty();
        await Assert.That(menu?.Id).IsEqualTo(menuId);
    }

    [Test]
    public async Task ProcessAsync_WithMoreLabel_RendersLabelOnTrigger()
    {
        // Arrange
        var sut = CreateSut();
        sut.MoreLabel = "Other trips";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=tabs-more]")?.TextContent)
            .IsEqualTo("Other trips");
    }

    [Test]
    public async Task ProcessAsync_WithOverflowNone_RendersNoMenu()
    {
        // Arrange
        var sut = CreateSut();
        sut.Overflow = TabListOverflow.None;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("div.sa-tabs")).IsNotNull();
        await Assert.That(html.QuerySelector("sel-tab-list")).IsNull();
        await Assert.That(html.QuerySelector("[data-slot=tabs-more]")).IsNull();
        await Assert.That(html.QuerySelector("sel-dropdown-menu")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WhenVertical_RendersNoMenu()
    {
        // Arrange
        var sut = CreateSut();
        sut.Orientation = TabListOrientation.Vertical;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("sel-tab-list")).IsNull();
        await Assert.That(html.QuerySelector("[data-slot=tabs-more]")).IsNull();
    }

    private static TabListTagHelper CreateSut() => new(Options.Create(new IconOptions()));
}
