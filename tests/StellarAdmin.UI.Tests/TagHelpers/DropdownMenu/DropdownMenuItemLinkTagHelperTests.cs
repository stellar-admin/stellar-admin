using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DropdownMenu;

public class DropdownMenuItemLinkTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenHrefIsSet_RendersLinkMenuItem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DropdownMenuItemLinkTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("href", "https://help.voyager.travel")]
        );

        // Assert
        var item = html.QuerySelector("[data-slot=dropdown-menu-item]");
        await Assert.That(item?.LocalName).IsEqualTo("a");
        await Assert.That(item?.GetAttribute("href")).IsEqualTo("https://help.voyager.travel");
        await Assert.That(item?.GetAttribute("role")).IsEqualTo("menuitem");
        await Assert.That(item?.GetAttribute("tabindex")).IsEqualTo("-1");
        await Assert.That(item?.GetAttribute("data-variant")).IsEqualTo("default");
        await Assert.That(item?.HasAttribute("type")).IsFalse();
        await Assert.That(item?.ClassList).Contains("sa-dropdown-menu-item");
    }

    [Test]
    public async Task ProcessAsync_WhenStateIsSet_RendersStateAttributes()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DropdownMenuItemLinkTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            CloseOnClick = false,
            Disabled = true,
            Inset = true,
            Variant = DropdownMenuItemVariant.Destructive,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("[data-slot=dropdown-menu-item]");
        await Assert.That(item?.GetAttribute("data-close-on-click")).IsEqualTo("false");
        await Assert.That(item?.HasAttribute("data-disabled")).IsTrue();
        await Assert.That(item?.GetAttribute("aria-disabled")).IsEqualTo("true");
        await Assert.That(item?.GetAttribute("data-inset")).IsEqualTo("true");
        await Assert.That(item?.GetAttribute("data-variant")).IsEqualTo("destructive");
    }

    [Test]
    public async Task ProcessAsync_WhenNoRouteTargetIsSet_RendersNoHref()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DropdownMenuItemLinkTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("[data-slot=dropdown-menu-item]");
        await Assert.That(item?.HasAttribute("href")).IsFalse();
    }
}
