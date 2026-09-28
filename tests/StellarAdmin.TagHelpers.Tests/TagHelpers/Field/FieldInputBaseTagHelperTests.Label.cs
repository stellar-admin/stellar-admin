using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    [Test]
    [Arguments("input", "input")]
    [Arguments("textarea", "textarea")]
    [Arguments("input-checkbox", "input")]
    [Arguments("switch", "input")]
    public async Task ProcessAsync_WhenUnboundWithLabel_TargetsGeneratedControlId(
        string kind,
        string controlSelector
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";
        sut.Label = "Label text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var controlId = html.QuerySelector(controlSelector)?.Id;
        await Assert.That(controlId).StartsWith("sa-");
        await Assert
            .That(html.QuerySelector("[data-slot=field-label]")?.GetAttribute("for"))
            .IsEqualTo(controlId);
    }

    [Test]
    public async Task ProcessAsync_WhenModelBound_TargetsModelControlIdWithModelLabel()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.For = CreateFor(context, "input");

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("input")?.Id).IsEqualTo("Value");
        await Assert
            .That(html.QuerySelector("[data-slot=field-label]")?.GetAttribute("for"))
            .IsEqualTo("Value");
        await Assert
            .That(html.QuerySelector("[data-slot=field-label]")?.TextContent)
            .IsEqualTo("Value");
    }
}
