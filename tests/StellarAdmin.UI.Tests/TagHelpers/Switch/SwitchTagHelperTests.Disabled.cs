using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Switch;

public partial class SwitchTagHelperTests
{
    [Test]
    public async Task ProcessAsync_DisabledBoundSwitch_LeavesOutHiddenFalseInput()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.CheckBoxHiddenInputRenderMode = CheckBoxHiddenInputRenderMode.Inline;
        var sut = new SwitchTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Notifications",
                context.Metadata.GetModelExplorerForType(typeof(bool), true)
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("disabled", "disabled")]
        );

        // Assert
        await Assert.That(html.QuerySelector("input[role=switch][disabled]")).IsNotNull();
        await Assert.That(html.QuerySelector("input[type=hidden]")).IsNull();
    }
}
