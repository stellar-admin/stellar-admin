using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Switch;

public partial class SwitchTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesClassesToExpectedElements()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SwitchTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            ClassNames = new SwitchClassNames { Control = "switch-control" },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".switch-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("span.sa-switch-wrapper.switch-control").Length)
            .IsEqualTo(1);
    }
}
