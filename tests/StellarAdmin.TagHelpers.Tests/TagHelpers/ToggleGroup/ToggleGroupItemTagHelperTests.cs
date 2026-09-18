using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.ToggleGroup;

public partial class ToggleGroupItemTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenExplicitlySelected_RendersCheckedInput()
    {
        // Arrange
        using var context = new RenderingContext();
        var parent = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            ClassNames = new ToggleGroupClassNames
            {
                Control = "toggle-control",
                Item = "toggle-item",
            },
        };
        var sut = new ToggleGroupItemTagHelper { Value = "one", Selected = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            parent,
            parentContext => TagHelperRenderer.RenderChildAsync(sut, parentContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("label input[value=one]")?.HasAttribute("checked"))
            .IsTrue();
    }
}
