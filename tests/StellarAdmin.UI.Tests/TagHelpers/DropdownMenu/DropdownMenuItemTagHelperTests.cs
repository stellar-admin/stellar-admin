using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DropdownMenu;

public class DropdownMenuItemTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenTypeIsNotSet_RendersNonSubmittingButton()
    {
        // Arrange
        var sut = new DropdownMenuItemTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("[data-slot=dropdown-menu-item]");
        await Assert.That(item?.LocalName).IsEqualTo("button");
        await Assert.That(item?.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(item?.GetAttribute("role")).IsEqualTo("menuitem");
        await Assert.That(item?.GetAttribute("tabindex")).IsEqualTo("-1");
        await Assert.That(item?.GetAttribute("data-variant")).IsEqualTo("default");
        await Assert.That(item?.ClassList).Contains("sa-dropdown-menu-item");
        await Assert.That(item?.ClassList).Contains("existing-class");
    }

    [Test]
    public async Task ProcessAsync_WhenTypeIsSubmit_KeepsSubmit()
    {
        // Arrange
        var sut = new DropdownMenuItemTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("type", "submit")]
        );

        // Assert
        var item = html.QuerySelector("[data-slot=dropdown-menu-item]");
        await Assert.That(item?.GetAttribute("type")).IsEqualTo("submit");
    }

    [Test]
    public async Task ProcessAsync_WhenStateIsSet_RendersStateAttributes()
    {
        // Arrange
        var sut = new DropdownMenuItemTagHelper
        {
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
    public async Task ProcessAsync_WhenStateIsNotSet_OmitsStateAttributes()
    {
        // Arrange
        var sut = new DropdownMenuItemTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("[data-slot=dropdown-menu-item]");
        await Assert.That(item?.HasAttribute("data-close-on-click")).IsFalse();
        await Assert.That(item?.HasAttribute("data-disabled")).IsFalse();
        await Assert.That(item?.HasAttribute("aria-disabled")).IsFalse();
        await Assert.That(item?.HasAttribute("data-inset")).IsFalse();
    }
}
