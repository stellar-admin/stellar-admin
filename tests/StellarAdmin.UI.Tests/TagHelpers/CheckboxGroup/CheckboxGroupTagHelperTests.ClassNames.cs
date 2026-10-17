using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.CheckboxGroup;

public partial class CheckboxGroupTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesClassesToExpectedElements()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "amenities",
            Label = "Amenities",
            Items = [new("Wi-Fi", "wifi"), new("Breakfast", "breakfast")],
            ClassNames = new ChoiceGroupClassNames
            {
                Control = "choice-control",
                Item = "choice-item",
                Label = "choice-label",
                Root = "choice-root",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("[data-slot=checkbox-group].choice-control").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=checkbox-group] > .choice-item").Length)
            .IsEqualTo(2);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-label].choice-label").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field].choice-root").Length)
            .IsEqualTo(1);
    }
}
