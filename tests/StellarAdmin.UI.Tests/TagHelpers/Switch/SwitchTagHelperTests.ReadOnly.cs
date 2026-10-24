using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Switch;

public partial class SwitchTagHelperTests
{
    [Test]
    public async Task ProcessAsync_ReadonlyAttribute_AddsAriaReadonlyAndCancelsClicks()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SwitchTagHelper(context.Generator) { ViewContext = context.ViewContext };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("readonly")]
        );

        // Assert
        var input = html.QuerySelector("input[role=switch]");
        await Assert.That(input?.HasAttribute("readonly")).IsTrue();
        await Assert.That(input?.GetAttribute("aria-readonly")).IsEqualTo("true");
        await Assert.That(input?.GetAttribute("onclick")).IsEqualTo("return false");
    }
}
