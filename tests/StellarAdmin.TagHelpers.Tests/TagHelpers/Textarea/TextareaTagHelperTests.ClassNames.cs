using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Textarea;

public partial class TextareaTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesClassesToExpectedElements()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TextareaTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            ClassNames = new TextareaClassNames { Control = "textarea-input" },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".textarea-input").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("textarea.sa-textarea.textarea-input").Length)
            .IsEqualTo(1);
    }
}
