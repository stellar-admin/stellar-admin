using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Input;

public partial class InputTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenModelBound_RendersClassAttributeContent()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue("Name", "submitted", "submitted");
        context.ViewContext.ViewData.ModelState.AddModelError("Name", "Required name.");
        context.ViewContext.ClientValidationEnabled = true;
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Name",
                context.Metadata.GetModelExplorerForType(typeof(string), "original")
            ),
            ClassNames = new InputClassNames { Control = "custom-control", Error = "custom-error" },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".custom-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("input.sa-input.custom-control").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.DocumentElement.OuterHtml)
            .DoesNotContain("ClassAttributeHtmlContent");
    }

    [Test]
    public async Task ProcessAsync_WhenModelStateContainsValue_UsesSubmittedValue()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue("Name", "submitted", "submitted");
        context.ViewContext.ViewData.ModelState.AddModelError("Name", "Required name.");
        context.ViewContext.ClientValidationEnabled = true;
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Name",
                context.Metadata.GetModelExplorerForType(typeof(string), "original")
            ),
            ClassNames = new InputClassNames { Control = "custom-control", Error = "custom-error" },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("input[name=Name]")?.GetAttribute("value"))
            .IsEqualTo("submitted");
    }

    [Test]
    public async Task ProcessAsync_WhenModelStateHasErrors_PreservesValidationClasses()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue("Name", "submitted", "submitted");
        context.ViewContext.ViewData.ModelState.AddModelError("Name", "Required name.");
        context.ViewContext.ClientValidationEnabled = true;
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Name",
                context.Metadata.GetModelExplorerForType(typeof(string), "original")
            ),
            ClassNames = new InputClassNames { Control = "custom-control", Error = "custom-error" },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".custom-error").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-field-error.custom-error").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelector(".field-validation-error")).IsNotNull();
    }
}
