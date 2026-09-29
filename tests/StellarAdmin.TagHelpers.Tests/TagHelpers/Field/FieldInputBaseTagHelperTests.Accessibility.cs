using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    [Test]
    [Arguments("input")]
    [Arguments("input-checkbox")]
    [Arguments("textarea")]
    [Arguments("select")]
    [Arguments("switch")]
    [Arguments("toggle")]
    [Arguments("input-otp")]
    public async Task ProcessAsync_WithDescriptionAndError_DescribesControlByBoth(string kind)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";
        sut.Description = "Description text";
        sut.Error = "Error text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var control = DescribedControl(html);
        var description = html.QuerySelector("[data-slot=field-description]");
        var error = html.QuerySelector("[data-slot=field-error]");
        await Assert.That(description?.Id).IsNotNull();
        await Assert.That(error?.Id).IsNotNull();
        await Assert
            .That(control?.GetAttribute("aria-describedby"))
            .IsEqualTo($"{description?.Id} {error?.Id}");
        await Assert.That(control?.GetAttribute("aria-invalid")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WithAuthorDescribedBy_KeepsAuthorIdsFirst()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Name = "value";
        sut.Description = "Description text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("aria-describedby", "hint")]
        );

        // Assert
        var description = html.QuerySelector("[data-slot=field-description]");
        await Assert
            .That(DescribedControl(html)?.GetAttribute("aria-describedby"))
            .IsEqualTo($"hint {description?.Id}");
    }

    [Test]
    public async Task ProcessAsync_WhenModelBoundWithoutDescription_DescribesControlByErrorOnly()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.For = CreateFor(context, "input");

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var error = html.QuerySelector("[data-slot=field-error]");
        await Assert.That(html.QuerySelector("[data-slot=field-description]")).IsNull();
        await Assert.That(error?.Id).IsNotNull();
        await Assert
            .That(DescribedControl(html)?.GetAttribute("aria-describedby"))
            .IsEqualTo(error?.Id);
        await Assert.That(DescribedControl(html)?.HasAttribute("aria-invalid")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenModelBoundRadio_DoesNotReferenceOmittedError()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input-radio", context);
        sut.For = CreateFor(context, "input-radio");

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("[data-slot=field-error]")).IsNull();
        await Assert.That(html.QuerySelector("[aria-describedby]")).IsNull();
    }

    [Test]
    [Arguments("input")]
    [Arguments("input-checkbox")]
    [Arguments("textarea")]
    [Arguments("select")]
    [Arguments("switch")]
    [Arguments("toggle")]
    [Arguments("input-otp")]
    public async Task ProcessAsync_WhenModelStateHasError_MarksControlInvalid(string kind)
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.AddModelError("Value", "Error text");
        var sut = CreateSut(kind, context);
        sut.For = CreateFor(context, kind);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(DescribedControl(html)?.GetAttribute("aria-invalid")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WithAuthorAriaInvalid_KeepsAuthorValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Name = "value";
        sut.Error = "Error text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("aria-invalid", "false")]
        );

        // Assert
        await Assert.That(DescribedControl(html)?.GetAttribute("aria-invalid")).IsEqualTo("false");
    }

    [Test]
    public async Task ProcessAsync_WithoutFieldWrapper_MarksInvalidWithoutDescribing()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Name = "value";
        sut.Description = "Description text";
        sut.Error = "Error text";
        sut.ShouldRenderField = false;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var control = DescribedControl(html);
        await Assert.That(control?.HasAttribute("aria-describedby")).IsFalse();
        await Assert.That(control?.GetAttribute("aria-invalid")).IsEqualTo("true");
    }

    [Test]
    [Arguments("radio-group", "[role=radiogroup]")]
    [Arguments("checkbox-group", "[role=group]")]
    [Arguments("segmented-control", "[role=radiogroup]")]
    [Arguments("toggle-group", "[role=group]")]
    [Arguments("slider", "[role=slider]")]
    public async Task ProcessAsync_WithDescriptionAndError_DescribesGroupByBoth(
        string kind,
        string selector
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";
        sut.Description = "Description text";
        sut.Error = "Error text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var description = FieldPart(html, "field-description");
        var error = FieldPart(html, "field-error");
        var group = html.QuerySelector(selector);
        await Assert.That(description?.Id).IsNotNull();
        await Assert.That(error?.Id).IsNotNull();
        await Assert
            .That(group?.GetAttribute("aria-describedby"))
            .IsEqualTo($"{description?.Id} {error?.Id}");
        await Assert.That(group?.GetAttribute("aria-invalid")).IsEqualTo("true");
    }

    [Test]
    [Arguments("radio-group")]
    [Arguments("checkbox-group")]
    [Arguments("segmented-control")]
    public async Task ProcessAsync_WithAuthorDescribedByOnGroup_DescribesGroupNotOptions(
        string kind
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                kind == "segmented-control"
                    ? TagHelperRenderer.RenderChildAsync(
                        new SegmentedControlItemTagHelper(context.Generator)
                        {
                            ViewContext = context.ViewContext,
                            Value = "1",
                        },
                        parent
                    )
                    : Task.FromResult(""),
            [new TagHelperAttribute("aria-describedby", "hint")]
        );

        // Assert
        await Assert
            .That(html.QuerySelectorAll("input:not([type=hidden])").Length)
            .IsGreaterThan(0);
        await Assert.That(html.QuerySelector("input[aria-describedby]")).IsNull();
        await Assert
            .That(html.QuerySelector("[role]")?.GetAttribute("aria-describedby"))
            .IsEqualTo("hint");
    }

    [Test]
    public async Task ProcessAsync_Slider_MovesDescriptionsFromHostToThumbs()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = (SliderTagHelper)CreateSut("slider", context);
        sut.Name = "value";
        sut.Value = "20,80";
        sut.Error = "Error text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("aria-describedby", "hint")]
        );

        // Assert
        var error = FieldPart(html, "field-error");
        var thumbs = html.QuerySelectorAll("[role=slider]");
        await Assert.That(thumbs.Length).IsEqualTo(2);
        foreach (var thumb in thumbs)
        {
            await Assert
                .That(thumb.GetAttribute("aria-describedby"))
                .IsEqualTo($"hint {error?.Id}");
            await Assert.That(thumb.GetAttribute("aria-invalid")).IsEqualTo("true");
        }

        var host = html.QuerySelector("sel-slider");
        await Assert.That(host?.HasAttribute("aria-describedby")).IsFalse();
        await Assert.That(host?.HasAttribute("aria-invalid")).IsFalse();
    }

    [Test]
    [Arguments("radio-group", "[role=radiogroup]")]
    [Arguments("segmented-control", "[role=radiogroup]")]
    [Arguments("toggle-group", "[role=group]")]
    [Arguments("slider", "[role=slider]")]
    public async Task ProcessAsync_WhenModelStateHasError_MarksGroupInvalid(
        string kind,
        string selector
    )
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.AddModelError("Value", "Error text");
        var sut = CreateSut(kind, context);
        sut.For = CreateFor(context, kind);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector(selector)?.GetAttribute("aria-invalid"))
            .IsEqualTo("true");
    }

    // A part of the automatic field wrapper itself, not of a choice group option's nested field
    private static IElement? FieldPart(IHtmlDocument html, string slot)
    {
        return html.Body?.FirstElementChild?.Children.FirstOrDefault(element =>
            element.GetAttribute("data-slot") == slot
        );
    }

    // The element a screen reader reaches: the real form control, never a hidden companion input
    private static IElement? DescribedControl(IHtmlDocument html)
    {
        return html.QuerySelector("input:not([type=hidden]), textarea, select");
    }
}
