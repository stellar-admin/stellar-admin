using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.DropdownMenu;

public class DropdownMenuContentTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenMenuHasId_UsesItForTriggerAndContent()
    {
        // Arrange
        var sut = new DropdownMenuContentTagHelper(
            Options.Create(new StellarAdminTagHelpersOptions())
        );

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
        var sut = new DropdownMenuContentTagHelper(
            Options.Create(new StellarAdminTagHelpersOptions())
        );

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
        var sut = new DropdownMenuContentTagHelper(
            Options.Create(new StellarAdminTagHelpersOptions())
        );

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
}
