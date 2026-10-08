using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Chip;

public class ChipRemoveTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithoutContent_RendersGhostIconButtonWithCloseIcon()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ChipRemoveTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("aria-label", "Remove Lisbon")]
        );

        // Assert
        var button = html.QuerySelector("button[data-slot=chip-remove]");
        await Assert.That(button?.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(button?.GetAttribute("aria-label")).IsEqualTo("Remove Lisbon");
        await Assert.That(button?.HasAttribute("disabled")).IsFalse();
        await Assert.That(button?.ClassList).Contains("sa-chip-remove");
        await Assert.That(button?.ClassList).Contains("sa-button-variant-ghost");
        await Assert.That(button?.ClassList).Contains("sa-button-size-icon-xs");
        await Assert.That(button?.ClassList).Contains("existing-class");
        await Assert.That(button?.QuerySelector("svg")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WithContent_RendersContentInsteadOfIcon()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ChipRemoveTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("<i>x</i>"),
            [new TagHelperAttribute("aria-label", "Remove Lisbon")]
        );

        // Assert
        var button = html.QuerySelector("[data-slot=chip-remove]");
        await Assert.That(button?.QuerySelector("i")).IsNotNull();
        await Assert.That(button?.QuerySelector("svg")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithoutAriaLabel_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ChipRemoveTagHelper(context.Icons);

        // Act
        var act = async () => await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
