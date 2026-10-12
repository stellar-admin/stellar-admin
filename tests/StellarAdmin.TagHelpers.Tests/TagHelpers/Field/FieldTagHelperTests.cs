using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public class FieldTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenOutsideGrid_RendersFieldOnly()
    {
        // Arrange
        var sut = new FieldTagHelper { ColumnSpan = "2" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.Body?.FirstElementChild?.GetAttribute("data-slot"))
            .IsEqualTo("field");
        await Assert.That(html.QuerySelector("[data-slot=grid-item]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WhenInsideGrid_WrapsFieldInGridItemWithSpan()
    {
        // Arrange
        var sut = new FieldTagHelper { ColumnSpan = "2", ColumnSpanLarge = "full" };
        var grid = new GridTagHelper { Columns = 3, ColumnsLarge = 4 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        var item = html.QuerySelector(".sa-grid-content > [data-slot=grid-item]");
        await Assert.That(item?.GetAttribute("style")).IsEqualTo("--sa-span-md:2;--sa-span-lg:4");
        await Assert.That(item?.QuerySelector(":scope > [data-slot=field]")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WhenNestedInGridItem_RendersFieldOnly()
    {
        // Arrange
        var sut = new FieldTagHelper { ColumnSpan = "2" };
        var item = new GridItemTagHelper();
        var grid = new GridTagHelper { Columns = 3 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext =>
                TagHelperRenderer.RenderChildAsync(
                    item,
                    gridContext,
                    itemContext => TagHelperRenderer.RenderChildAsync(sut, itemContext)
                )
        );

        // Assert
        await Assert.That(html.QuerySelectorAll("[data-slot=grid-item]").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector("[data-slot=grid-item]")?.HasAttribute("style"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenSpanIsInvalid_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new FieldTagHelper { ColumnSpan = "13" };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
