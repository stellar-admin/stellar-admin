using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    [Test]
    [Arguments("input")]
    [Arguments("select")]
    [Arguments("switch")]
    [Arguments("textarea")]
    public async Task ProcessAsync_WhenInsideGrid_WrapsFieldInGridItemWithSpan(string kind)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(kind, context);
        sut.Label = "Destination";
        sut.ColumnSpan = "full";
        var grid = new GridTagHelper { ColumnsMedium = 2 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        var content = html.QuerySelector(".sa-grid-content");
        await Assert.That(content?.Children.Length).IsEqualTo(1);
        var item = content?.FirstElementChild;
        await Assert.That(item?.GetAttribute("data-slot")).IsEqualTo("grid-item");
        await Assert.That(item?.GetAttribute("style")).IsEqualTo("--sa-span-md:2;--sa-span-lg:2");
        await Assert.That(item?.QuerySelector(":scope > [data-slot=field]")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WhenInsideGridWithStart_WritesStartOnGridItem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Label = "Destination";
        sut.ColumnStartMedium = "2";
        var grid = new GridTagHelper { ColumnsMedium = 2 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=grid-item]")?.GetAttribute("style"))
            .IsEqualTo("--sa-start-md:2;--sa-start-lg:2");
    }

    [Test]
    public async Task ProcessAsync_WhenInsideGridWithoutField_WrapsControlInGridItem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Label = "Destination";
        sut.WrapInField = false;
        sut.ColumnSpan = "2";
        var grid = new GridTagHelper { ColumnsMedium = 3 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        var item = html.QuerySelector(".sa-grid-content > [data-slot=grid-item]");
        await Assert.That(item?.GetAttribute("style")).IsEqualTo("--sa-span-md:2;--sa-span-lg:2");
        await Assert.That(item?.QuerySelector(":scope > input")).IsNotNull();
        await Assert.That(html.QuerySelector("[data-slot=field]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WhenInsideFieldInGrid_RendersControlOnly()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.ColumnSpan = "2";
        var field = new FieldTagHelper();
        var grid = new GridTagHelper { ColumnsMedium = 3 };
        var control = string.Empty;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            async gridContext =>
            {
                // The field's children render inside the field's scope
                var fieldContext = new TagHelperContext(
                    new TagHelperAttributeList(),
                    new Dictionary<object, object>(gridContext.Items),
                    "field"
                );
                field.Init(fieldContext);
                control = await TagHelperRenderer.RenderChildAsync(sut, fieldContext);

                return control;
            }
        );

        // Assert
        await Assert.That(control).StartsWith("<input");
        await Assert.That(control).DoesNotContain("grid-item");
    }

    [Test]
    public async Task ProcessAsync_WhenOutsideGridWithSpan_RendersNoGridItem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Label = "Destination";
        sut.ColumnSpan = "2";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("[data-slot=grid-item]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WhenSpanIsInvalid_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut("input", context);
        sut.Label = "Destination";
        sut.ColumnSpanSmall = "half";

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
