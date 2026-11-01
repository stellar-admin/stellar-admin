using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.PageHeader;

public class PageHeaderTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenSticky_AddsStickyAttribute()
    {
        // Arrange
        var sut = new PageHeaderTagHelper { Sticky = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var header = html.QuerySelector("header[data-slot=page-header]");
        await Assert.That(header?.HasAttribute("data-sticky")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_ByDefault_LeavesOutStickyAttribute()
    {
        // Arrange
        var sut = new PageHeaderTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var header = html.QuerySelector("header[data-slot=page-header]");
        await Assert.That(header).IsNotNull();
        await Assert.That(header?.HasAttribute("data-sticky")).IsFalse();
    }
}
