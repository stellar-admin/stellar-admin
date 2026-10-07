using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.SegmentedControl;

public class SegmentedControlItemTagHelperTests
{
    [Test]
    public async Task ProcessAsync_SelectedWithNullBinding_ChecksItem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SegmentedControlTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Cabin",
                context.Metadata.GetModelExplorerForType(typeof(int?), null)
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async parent =>
                await TagHelperRenderer.RenderChildAsync(
                    new SegmentedControlItemTagHelper(context.Generator)
                    {
                        ViewContext = context.ViewContext,
                        Value = "",
                        Selected = true,
                    },
                    parent
                )
                + await TagHelperRenderer.RenderChildAsync(
                    new SegmentedControlItemTagHelper(context.Generator)
                    {
                        ViewContext = context.ViewContext,
                        Value = "1",
                    },
                    parent
                )
        );

        // Assert
        await Assert.That(html.QuerySelector("input:checked")?.GetAttribute("value")).IsEqualTo("");
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(1);
    }

    [Test]
    public async Task ProcessAsync_SelectedFalse_OverridesInitialValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SegmentedControlTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "cabin",
            Value = "1",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new SegmentedControlItemTagHelper(context.Generator)
                    {
                        ViewContext = context.ViewContext,
                        Value = "1",
                        Selected = false,
                    },
                    parent
                )
        );

        // Assert
        await Assert.That(html.QuerySelector("input:checked")).IsNull();
    }
}
