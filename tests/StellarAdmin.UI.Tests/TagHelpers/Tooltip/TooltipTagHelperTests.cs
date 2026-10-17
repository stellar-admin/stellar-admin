using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Tooltip;

public class TooltipTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithNoPlacement_PlacesAboveAndCentered()
    {
        // Arrange
        var sut = new TooltipTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var tooltip = html.QuerySelector("[data-slot=tooltip-content]");
        await Assert.That(tooltip?.ClassList.Contains("anchored-top-center")).IsTrue();
        await Assert.That(tooltip?.GetAttribute("data-anchor-side")).IsEqualTo("top");
    }

    [Test]
    public async Task ProcessAsync_WithSideAndAlign_WritesPlacement()
    {
        // Arrange
        var sut = new TooltipTagHelper { Side = TooltipSide.Left, Align = TooltipAlign.End };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var tooltip = html.QuerySelector("[data-slot=tooltip-content]");
        await Assert.That(tooltip?.ClassList.Contains("anchored-left-end")).IsTrue();
        await Assert.That(tooltip?.GetAttribute("data-anchor-side")).IsEqualTo("left");
    }
}
