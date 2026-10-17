using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DropdownMenu;

public class DropdownMenuSubContentTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenSubHasId_UsesItForSubTriggerAndContent()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DropdownMenuSubContentTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new DropdownMenuSubTagHelper(),
            async sub =>
                await TagHelperRenderer.RenderChildAsync(
                    new DropdownMenuSubTriggerTagHelper(context.Icons),
                    sub
                ) + await TagHelperRenderer.RenderChildAsync(sut, sub),
            [new TagHelperAttribute("id", "--trip-share")]
        );

        // Assert
        var trigger = html.QuerySelector("[data-slot=dropdown-menu-sub-trigger]");
        var content = html.QuerySelector("[data-slot=dropdown-menu-sub-content]");
        await Assert.That(trigger?.GetAttribute("popovertarget")).IsEqualTo("--trip-share");
        await Assert.That(content?.Id).IsEqualTo("--trip-share");
    }

    [Test]
    public async Task ProcessAsync_WhenContentHasItsOwnId_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new DropdownMenuSubContentTagHelper();

        // Act
        var render = async () =>
            await TagHelperRenderer.RenderAsync(
                new DropdownMenuSubTagHelper(),
                sub =>
                    TagHelperRenderer.RenderChildAsync(
                        sut,
                        sub,
                        outputAttributes: [new TagHelperAttribute("id", "--trip-share")]
                    )
            );

        // Assert
        await Assert.That(render).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WithNoPlacement_UsesDefaultPlacement()
    {
        // Arrange
        var sut = new DropdownMenuSubContentTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var content = html.QuerySelector("[data-slot=dropdown-menu-sub-content]");
        await Assert.That(content?.ClassList.Contains("anchored-right-start")).IsTrue();
        await Assert.That(content?.GetAttribute("data-side")).IsEqualTo("right");
        await Assert.That(content?.GetAttribute("data-anchor-side")).IsEqualTo("right");
    }

    [Test]
    public async Task ProcessAsync_WithSideAndAlign_WritesPlacement()
    {
        // Arrange
        var sut = new DropdownMenuSubContentTagHelper()
        {
            Side = DropdownMenuSide.Left,
            Align = DropdownMenuAlign.End,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var content = html.QuerySelector("[data-slot=dropdown-menu-sub-content]");
        await Assert.That(content?.ClassList.Contains("anchored-left-end")).IsTrue();
        await Assert.That(content?.GetAttribute("data-side")).IsEqualTo("left");
        await Assert.That(content?.GetAttribute("data-anchor-side")).IsEqualTo("left");
    }
}
