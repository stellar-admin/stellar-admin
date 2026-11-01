using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Button;

public class ButtonTagHelperTests
{
    [Test]
    public async Task ProcessAsync_OutlineDestructiveVariant_AddsOutlineDestructiveClass()
    {
        // Arrange
        var sut = new ButtonTagHelper { Variant = ButtonVariant.OutlineDestructive };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var button = html.QuerySelector("button[data-slot=button]");
        await Assert
            .That(button?.ClassList.Contains("sa-button-variant-outline-destructive"))
            .IsTrue();
        await Assert.That(button?.ClassList.Contains("sa-button-variant-outline")).IsFalse();
    }
}
