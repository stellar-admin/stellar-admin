using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.ToggleGroup;

public partial class ToggleGroupTagHelperTests
{
    [Test]
    [Arguments(null, "2", "--gap: 2")]
    [Arguments(ToggleGroupSpacing.None, "0", null)]
    [Arguments(ToggleGroupSpacing.Small, "1", "--gap: 1")]
    [Arguments(ToggleGroupSpacing.Large, "4", "--gap: 4")]
    public async Task ProcessAsync_Spacing_RendersSpacingUnitsAndGap(
        ToggleGroupSpacing? spacing,
        string expectedUnits,
        string? expectedStyle
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "view",
            Spacing = spacing,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent => RenderItemsAsync(parent, new ToggleGroupItemTagHelper { Value = "list" })
        );

        // Assert
        var group = html.QuerySelector("[data-slot=toggle-group]")!;
        await Assert.That(group.GetAttribute("data-spacing")).IsEqualTo(expectedUnits);
        await Assert.That(group.GetAttribute("style")).IsEqualTo(expectedStyle);
        await Assert
            .That(html.QuerySelector("[data-slot=toggle-group-item]")?.GetAttribute("data-spacing"))
            .IsEqualTo(expectedUnits);
    }
}
