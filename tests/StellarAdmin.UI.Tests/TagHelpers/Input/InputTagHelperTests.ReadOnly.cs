using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Input;

public partial class InputTagHelperTests
{
    [Test]
    [Arguments("checkbox", "true", "return false")]
    [Arguments("radio", null, "return false")]
    [Arguments("text", null, null)]
    public async Task ProcessAsync_ReadonlyAttribute_CancelsChoiceClicks(
        string type,
        string? ariaReadonly,
        string? onclick
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            InputTypeName = type,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("readonly")]
        );

        // Assert
        var input = html.QuerySelector("input");
        await Assert.That(input?.HasAttribute("readonly")).IsTrue();
        await Assert.That(input?.GetAttribute("aria-readonly")).IsEqualTo(ariaReadonly);
        await Assert.That(input?.GetAttribute("onclick")).IsEqualTo(onclick);
    }
}
