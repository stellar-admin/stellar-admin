using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Select;

public partial class SelectTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesClassesToExpectedElements()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SelectTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            ClassNames = new SelectClassNames { Control = "select-control" },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".select-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-native-select-wrapper.select-control").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".existing-class").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-native-select-wrapper.existing-class").Length)
            .IsEqualTo(1);
    }
}
