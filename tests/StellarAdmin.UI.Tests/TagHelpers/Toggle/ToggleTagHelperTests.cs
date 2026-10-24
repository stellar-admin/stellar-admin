using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
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

    [Test]
    public async Task ProcessAsync_ReadonlyAttribute_AddsAriaReadonlyAndCancelsClicks()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "pinned",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("readonly")]
        );

        // Assert
        var input = html.QuerySelector("input[type=checkbox]");
        await Assert.That(input?.HasAttribute("readonly")).IsTrue();
        await Assert.That(input?.GetAttribute("aria-readonly")).IsEqualTo("true");
        await Assert.That(input?.GetAttribute("onclick")).IsEqualTo("return false");
    }

    [Test]
    public async Task ProcessAsync_DisabledBoundToggle_LeavesOutHiddenFalseInput()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.CheckBoxHiddenInputRenderMode = CheckBoxHiddenInputRenderMode.Inline;
        var sut = new ToggleTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Pinned",
                context.Metadata.GetModelExplorerForType(typeof(bool), true)
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("disabled", "disabled")]
        );

        // Assert
        await Assert.That(html.QuerySelector("input[type=checkbox][disabled]")).IsNotNull();
        await Assert.That(html.QuerySelector("input[type=hidden]")).IsNull();
    }
}
