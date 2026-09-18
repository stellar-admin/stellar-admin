using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.ToggleGroup;

public partial class ToggleGroupTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenControlClassIsConfigured_AppliesClassToContainer()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            ClassNames = new ToggleGroupClassNames
            {
                Control = "toggle-control",
                Item = "toggle-item",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".toggle-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-toggle-group.toggle-control").Length)
            .IsEqualTo(1);
    }
}
