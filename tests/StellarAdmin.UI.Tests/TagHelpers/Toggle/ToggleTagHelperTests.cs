using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Toggle;

public class ToggleTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesClassesToExpectedElements()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "pinned",
            Label = "Pinned",
            ClassNames = new ToggleClassNames
            {
                Control = "toggle-control",
                Label = "toggle-label",
                Root = "toggle-root",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("label[data-slot=toggle].toggle-control").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-label].toggle-label").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field].toggle-root").Length)
            .IsEqualTo(1);
    }
}
