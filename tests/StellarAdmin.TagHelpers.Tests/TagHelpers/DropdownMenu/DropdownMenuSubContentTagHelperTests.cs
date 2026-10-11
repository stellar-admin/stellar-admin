using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.DropdownMenu;

public class DropdownMenuSubContentTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenSubHasId_UsesItForSubTriggerAndContent()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DropdownMenuSubContentTagHelper(
            Options.Create(new StellarAdminTagHelpersOptions())
        );

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
        var sut = new DropdownMenuSubContentTagHelper(
            Options.Create(new StellarAdminTagHelpersOptions())
        );

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
}
