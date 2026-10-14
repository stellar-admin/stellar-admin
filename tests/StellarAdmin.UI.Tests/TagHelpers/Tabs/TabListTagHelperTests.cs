using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Tabs;

public class TabListTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithLinkTabs_RendersNoTabListRole()
    {
        // Arrange
        var sut = new TabListTagHelper();

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
        var sut = new TabListTagHelper { Orientation = TabListOrientation.Vertical };

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
}
