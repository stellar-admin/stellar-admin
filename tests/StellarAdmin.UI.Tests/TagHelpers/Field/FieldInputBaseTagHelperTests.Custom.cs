using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    [Test]
    public async Task ProcessAsync_CustomInputWhenModelBound_TargetsModelControlId()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ColorInputTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = CreateFor(context, "input"),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var control = html.QuerySelector("input");
        await Assert.That(control?.GetAttribute("type")).IsEqualTo("color");
        await Assert.That(control?.Id).IsEqualTo("Value");
        await Assert
            .That(html.QuerySelector("[data-slot=field-label]")?.GetAttribute("for"))
            .IsEqualTo("Value");
    }

    [Test]
    public async Task ProcessAsync_CustomInputWithDescriptionAndError_DescribesControlByBoth()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ColorInputTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "value",
            Label = "Colour",
            Description = "Description text",
            Error = "Error text",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var control = html.QuerySelector("input");
        var description = html.QuerySelector("[data-slot=field-description]");
        var error = html.QuerySelector("[data-slot=field-error]");
        await Assert
            .That(DescribeField(html))
            .IsEqualTo("field-label control field-error field-description");
        await Assert
            .That(control?.GetAttribute("aria-describedby"))
            .IsEqualTo($"{description?.Id} {error?.Id}");
        await Assert.That(control?.GetAttribute("aria-invalid")).IsEqualTo("true");
        await Assert
            .That(html.QuerySelector("[data-slot=field-label]")?.GetAttribute("for"))
            .IsEqualTo(control?.Id);
    }

    // The example from the consumer forms guide, built only on the public API.
    private sealed class ColorInputTagHelper(IHtmlGenerator generator)
        : FieldInputBaseTagHelper(generator)
    {
        protected override Task<FieldLayout> RenderInput(
            TagHelperContext context,
            TagHelperOutput output,
            IDictionary<string, object?>? htmlAttributes
        )
        {
            output.TagName = "input";
            output.TagMode = TagMode.SelfClosing;
            if (For != null)
            {
                output.MergeAttributes(
                    generator.GenerateTextBox(
                        ViewContext,
                        For.ModelExplorer,
                        For.Name,
                        For.Model,
                        null,
                        htmlAttributes
                    )
                );
            }

            output.Attributes.SetAttribute("type", "color");
            ApplyFieldAttributes(context, output.Attributes, FieldLayout.Stacked);
            return Task.FromResult(FieldLayout.Stacked);
        }
    }
}
