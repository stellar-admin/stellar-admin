using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DropdownMenu;

public class DropdownMenuContentTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenMenuHasId_UsesItForTriggerAndContent()
    {
        // Arrange
        var sut = new DropdownMenuContentTagHelper(Options.Create(new StellarAdminUIOptions()));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new DropdownMenuTagHelper(),
            async menu =>
                await TagHelperRenderer.RenderChildAsync(new DropdownMenuTriggerTagHelper(), menu)
                + await TagHelperRenderer.RenderChildAsync(sut, menu),
            [new TagHelperAttribute("id", "trip-menu")]
        );

        // Assert
        var trigger = html.QuerySelector("[data-slot=dropdown-menu-trigger]");
        var content = html.QuerySelector("[data-slot=dropdown-menu-content]");
        await Assert.That(trigger?.GetAttribute("popovertarget")).IsEqualTo("trip-menu");
        await Assert.That(content?.Id).IsEqualTo("trip-menu");
    }

    [Test]
    public async Task ProcessAsync_WhenContentRepeatsMenuId_Renders()
    {
        // Arrange
        var sut = new DropdownMenuContentTagHelper(Options.Create(new StellarAdminUIOptions()));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            new DropdownMenuTagHelper(),
            menu =>
                TagHelperRenderer.RenderChildAsync(
                    sut,
                    menu,
                    outputAttributes: [new TagHelperAttribute("id", "trip-menu")]
                ),
            [new TagHelperAttribute("id", "trip-menu")]
        );

        // Assert
        var content = html.QuerySelector("[data-slot=dropdown-menu-content]");
        await Assert.That(content?.Id).IsEqualTo("trip-menu");
    }

    [Test]
    public async Task ProcessAsync_WhenContentHasItsOwnId_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new DropdownMenuContentTagHelper(Options.Create(new StellarAdminUIOptions()));

        // Act
        var render = async () =>
            await TagHelperRenderer.RenderAsync(
                new DropdownMenuTagHelper(),
                menu =>
                    TagHelperRenderer.RenderChildAsync(
                        sut,
                        menu,
                        outputAttributes: [new TagHelperAttribute("id", "trip-menu")]
                    )
            );

        // Assert
        await Assert.That(render).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WithNoPlacement_UsesDefaultPlacement()
    {
        // Arrange
        var sut = new DropdownMenuContentTagHelper(Options.Create(new StellarAdminUIOptions()));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var content = html.QuerySelector("[data-slot=dropdown-menu-content]");
        await Assert.That(content?.ClassList.Contains("anchored-bottom-start")).IsTrue();
        await Assert.That(content?.GetAttribute("data-side")).IsEqualTo("bottom");
        await Assert.That(content?.GetAttribute("data-anchor-side")).IsEqualTo("bottom");
    }

    [Test]
    public async Task ProcessAsync_WithSideAndAlign_WritesPlacement()
    {
        // Arrange
        var sut = new DropdownMenuContentTagHelper(Options.Create(new StellarAdminUIOptions()))
        {
            Side = DropdownMenuSide.Right,
            Align = DropdownMenuAlign.End,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var content = html.QuerySelector("[data-slot=dropdown-menu-content]");
        await Assert.That(content?.ClassList.Contains("anchored-right-end")).IsTrue();
        await Assert.That(content?.GetAttribute("data-side")).IsEqualTo("right");
        await Assert.That(content?.GetAttribute("data-anchor-side")).IsEqualTo("right");
    }
}
