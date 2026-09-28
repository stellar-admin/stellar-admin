using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    [Test]
    [Arguments("input", "vertical", "field-label control field-error field-description")]
    [Arguments("textarea", "vertical", "field-label control field-error field-description")]
    [Arguments("select", "vertical", "field-label control field-error field-description")]
    [Arguments("input-otp", "vertical", "field-label control field-error field-description")]
    [Arguments("slider", "vertical", "field-label control field-error field-description")]
    [Arguments("toggle", "vertical", "field-label control field-error field-description")]
    [Arguments("toggle-group", "vertical", "field-label control field-error field-description")]
    [Arguments(
        "segmented-control",
        "vertical",
        "field-label control field-error field-description"
    )]
    [Arguments("checkbox-group", "vertical", "field-label field-description control field-error")]
    [Arguments("radio-group", "vertical", "field-label field-description control field-error")]
    [Arguments(
        "input-checkbox",
        "horizontal",
        "control field-content[field-label field-description field-error]"
    )]
    [Arguments(
        "input-radio",
        "horizontal",
        "control field-content[field-label field-description field-error]"
    )]
    [Arguments(
        "switch",
        "horizontal",
        "control field-content[field-label field-description field-error]"
    )]
    public async Task ProcessAsync_WhenUnboundWithAllParts_ArrangesFieldForControl(
        string kind,
        string orientation,
        string structure
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";
        sut.Label = "Label text";
        sut.Description = "Description text";
        sut.Error = "Error text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=field]")?.GetAttribute("data-orientation"))
            .IsEqualTo(orientation);
        await Assert.That(DescribeField(html)).IsEqualTo(structure);
    }

    [Test]
    [Arguments("input", "vertical", "field-label control field-error field-description")]
    [Arguments("textarea", "vertical", "field-label control field-error field-description")]
    [Arguments("select", "vertical", "field-label control field-error field-description")]
    [Arguments("input-otp", "vertical", "field-label control field-error field-description")]
    [Arguments("slider", "vertical", "field-label control field-error field-description")]
    [Arguments("toggle", "vertical", "field-label control field-error field-description")]
    [Arguments("toggle-group", "vertical", "field-label control field-error field-description")]
    [Arguments(
        "segmented-control",
        "vertical",
        "field-label control field-error field-description"
    )]
    [Arguments("checkbox-group", "vertical", "field-label field-description control field-error")]
    [Arguments("radio-group", "vertical", "field-label field-description control field-error")]
    [Arguments(
        "input-checkbox",
        "horizontal",
        "control field-content[field-label field-description field-error]"
    )]
    [Arguments("input-radio", "horizontal", "control field-content[field-label field-description]")]
    [Arguments(
        "switch",
        "horizontal",
        "control field-content[field-label field-description field-error]"
    )]
    public async Task ProcessAsync_WhenModelBoundWithAllParts_ArrangesFieldForControl(
        string kind,
        string orientation,
        string structure
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.For = CreateFor(context, kind);
        sut.Label = "Label text";
        sut.Description = "Description text";
        sut.Error = "Error text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=field]")?.GetAttribute("data-orientation"))
            .IsEqualTo(orientation);
        await Assert.That(DescribeField(html)).IsEqualTo(structure);
    }

    [Test]
    [Arguments("input", "field-label control field-error")]
    [Arguments("checkbox-group", "field-label control field-error")]
    [Arguments("input-checkbox", "control field-content[field-label field-error]")]
    [Arguments("input-radio", "control field-content[field-label]")]
    public async Task ProcessAsync_WhenModelBoundWithoutText_RendersPartsTheModelSupplies(
        string kind,
        string structure
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.For = CreateFor(context, kind);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(DescribeField(html)).IsEqualTo(structure);
    }

    [Test]
    [Arguments("input", "field-label control")]
    [Arguments("checkbox-group", "field-label control")]
    [Arguments("input-checkbox", "control field-content[field-label]")]
    [Arguments("switch", "control field-content[field-label]")]
    public async Task ProcessAsync_WhenUnboundWithLabelOnly_OmitsUnsuppliedParts(
        string kind,
        string structure
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
        await Assert.That(DescribeField(html)).IsEqualTo(structure);
    }
}
