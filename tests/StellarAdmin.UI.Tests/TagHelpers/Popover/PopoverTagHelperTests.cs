using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Popover;

public class PopoverTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithNoPlacement_PlacesBelowAndCentered()
    {
        // Arrange
        var sut = new PopoverTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var popover = html.QuerySelector("[data-slot=popover-content]");
        await Assert.That(popover?.ClassList.Contains("anchored-bottom-center")).IsTrue();
        await Assert.That(popover?.GetAttribute("data-anchor-side")).IsEqualTo("bottom");
    }

    [Test]
    [Arguments(PopoverSide.Top, PopoverAlign.Start, "anchored-top-start", "top")]
    [Arguments(PopoverSide.Bottom, PopoverAlign.End, "anchored-bottom-end", "bottom")]
    [Arguments(PopoverSide.Left, PopoverAlign.Center, "anchored-left-center", "left")]
    [Arguments(
        PopoverSide.Right,
        PopoverAlign.CenterWithin,
        "anchored-right-center-within",
        "right"
    )]
    public async Task ProcessAsync_WithSideAndAlign_WritesPlacement(
        PopoverSide side,
        PopoverAlign align,
        string expectedClass,
        string expectedSide
    )
    {
        // Arrange
        var sut = new PopoverTagHelper { Side = side, Align = align };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var popover = html.QuerySelector("[data-slot=popover-content]");
        await Assert.That(popover?.ClassList.Contains(expectedClass)).IsTrue();
        await Assert.That(popover?.GetAttribute("data-anchor-side")).IsEqualTo(expectedSide);
    }
}
