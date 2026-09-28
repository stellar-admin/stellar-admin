using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    [Test]
    [Arguments("input")]
    [Arguments("input-checkbox")]
    [Arguments("switch")]
    [Arguments("checkbox-group")]
    public async Task ProcessAsync_WhenNoFieldPartsOrModelAreSupplied_RendersControlOnly(
        string kind
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(DescribeField(html)).IsEqualTo("(no field)");
    }

    [Test]
    [Arguments("label")]
    [Arguments("description")]
    [Arguments("error")]
    public async Task ProcessAsync_WhenAnyFieldPartIsSupplied_RendersFieldWrapper(string part)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Name = "value";
        switch (part)
        {
            case "label":
                sut.Label = "Label text";
                break;
            case "description":
                sut.Description = "Description text";
                break;
            case "error":
                sut.Error = "Error text";
                break;
        }

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(DescribeField(html)).Contains($"field-{part}");
    }

    [Test]
    [Arguments("input")]
    [Arguments("input-checkbox")]
    [Arguments("switch")]
    [Arguments("checkbox-group")]
    public async Task ProcessAsync_WhenRenderFieldIsFalse_RendersControlWithoutParts(string kind)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";
        sut.Label = "Label text";
        sut.Description = "Description text";
        sut.Error = "Error text";
        sut.ShouldRenderField = false;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(DescribeField(html)).IsEqualTo("(no field)");
        await Assert.That(html.Body?.TextContent).DoesNotContain("Label text");
        await Assert.That(html.Body?.TextContent).DoesNotContain("Description text");
        await Assert.That(html.Body?.TextContent).DoesNotContain("Error text");
    }

    [Test]
    [Arguments("input", "control")]
    [Arguments("switch", "control field-content[]")]
    public async Task ProcessAsync_WhenRenderFieldIsTrueWithoutParts_RendersEmptyArrangement(
        string kind,
        string structure
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Name = "value";
        sut.ShouldRenderField = true;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(DescribeField(html)).IsEqualTo(structure);
    }

    [Test]
    public async Task ProcessAsync_WhenNestedInField_DoesNotRenderAnotherWrapper()
    {
        // Arrange
        using var context = new RenderingContext();
        var field = new FieldTagHelper();
        var sut = CreateSut("input", context);
        sut.Label = "Label text";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            field,
            parent => TagHelperRenderer.RenderChildAsync(sut, parent)
        );

        // Assert
        await Assert.That(html.QuerySelectorAll("[data-slot=field]").Length).IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll("[data-slot=field-label]").Length).IsEqualTo(0);
    }
}
