using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.SegmentedControl;

public class SegmentedControlTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesClassesToExpectedElements()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SegmentedControlTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "cabin",
            Label = "Cabin",
            ClassNames = new SegmentedControlClassNames
            {
                Control = "segmented-control",
                Item = "segmented-item",
                Label = "segmented-label",
                Root = "segmented-root",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async parent =>
                await TagHelperRenderer.RenderChildAsync(
                    new SegmentedControlItemTagHelper(context.Generator)
                    {
                        ViewContext = context.ViewContext,
                        Value = "economy",
                    },
                    parent
                )
                + await TagHelperRenderer.RenderChildAsync(
                    new SegmentedControlItemTagHelper(context.Generator)
                    {
                        ViewContext = context.ViewContext,
                        Value = "business",
                    },
                    parent
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelectorAll("[data-slot=segmented-control].segmented-control").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=segmented-control-item].segmented-item").Length)
            .IsEqualTo(2);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-label].segmented-label").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field].segmented-root").Length)
            .IsEqualTo(1);
    }
}
