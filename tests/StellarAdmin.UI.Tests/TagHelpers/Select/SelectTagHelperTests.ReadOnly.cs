using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Select;

public partial class SelectTagHelperTests
{
    [Test]
    public async Task ProcessAsync_ReadonlyAttribute_AddsAriaReadonlyAndKeepsItClosed()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SelectTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("readonly")]
        );

        // Assert
        var select = html.QuerySelector("select");
        await Assert.That(select?.HasAttribute("readonly")).IsTrue();
        await Assert.That(select?.GetAttribute("aria-readonly")).IsEqualTo("true");
        await Assert
            .That(select?.GetAttribute("onmousedown"))
            .IsEqualTo("this.focus(); return false");
        await Assert.That(select?.GetAttribute("onkeydown")).Contains("event.key");
    }
}
