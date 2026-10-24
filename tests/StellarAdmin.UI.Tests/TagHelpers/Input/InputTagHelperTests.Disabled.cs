using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Input;

public partial class InputTagHelperTests
{
    [Test]
    public async Task ProcessAsync_BoundCheckbox_RendersHiddenFalseInput()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.CheckBoxHiddenInputRenderMode = CheckBoxHiddenInputRenderMode.Inline;
        var sut = CreateBoundCheckbox(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("input[type=hidden][name=Accepted]")?.GetAttribute("value"))
            .IsEqualTo("false");
    }

    [Test]
    public async Task ProcessAsync_DisabledBoundCheckbox_LeavesOutHiddenFalseInput()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.CheckBoxHiddenInputRenderMode = CheckBoxHiddenInputRenderMode.Inline;
        var sut = CreateBoundCheckbox(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("disabled", "disabled")]
        );

        // Assert
        await Assert.That(html.QuerySelector("input[type=checkbox][disabled]")).IsNotNull();
        await Assert.That(html.QuerySelector("input[type=hidden]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_DisabledBoundCheckboxInForm_LeavesOutEndOfFormInput()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.CheckBoxHiddenInputRenderMode = CheckBoxHiddenInputRenderMode.EndOfForm;
        context.ViewContext.FormContext.CanRenderAtEndOfForm = true;
        var sut = CreateBoundCheckbox(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("disabled", "disabled")]
        );

        // Assert
        await Assert.That(context.ViewContext.FormContext.EndOfFormContent).IsEmpty();
    }

    private static InputTagHelper CreateBoundCheckbox(RenderingContext context) =>
        new(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Accepted",
                context.Metadata.GetModelExplorerForType(typeof(bool), true)
            ),
        };
}
