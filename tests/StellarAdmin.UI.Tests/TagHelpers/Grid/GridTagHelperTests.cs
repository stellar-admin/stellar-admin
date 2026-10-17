using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Grid;

public class GridTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenRendered_WrapsChildrenInGridContent()
    {
        // Arrange
        var sut = new GridTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("<div id=\"child\"></div>")
        );

        // Assert
        var root = html.Body?.FirstElementChild;
        await Assert.That(root?.GetAttribute("data-slot")).IsEqualTo("grid");
        await Assert.That(root?.ClassName).IsEqualTo("sa-grid existing-class");
        await Assert
            .That(
                html.QuerySelector(
                    "[data-slot=grid] > div.sa-grid-content[data-slot=grid-content] > #child"
                )
            )
            .IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WhenColumnsAreOmitted_RendersOneColumn()
    {
        // Arrange
        var sut = new GridTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var content = html.QuerySelector(".sa-grid-content");
        await Assert.That(content?.HasAttribute("style")).IsFalse();
        await Assert.That(content?.GetAttribute("data-columns")).IsEqualTo("single");
    }

    [Test]
    public async Task ProcessAsync_WhenAnyBreakpointHasMoreThanOneColumn_MarksMultipleColumns()
    {
        // Arrange
        var sut = new GridTagHelper { ColumnsLarge = 2 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector(".sa-grid-content")?.GetAttribute("data-columns"))
            .IsEqualTo("multiple");
    }

    [Test]
    [Arguments(null, null, 3, null, "--sa-cols-md:3;--sa-cols-lg:3")]
    [Arguments(null, 2, 3, 4, "--sa-cols-sm:2;--sa-cols-md:3;--sa-cols-lg:4")]
    [Arguments(2, null, null, null, "--sa-cols:2;--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:2")]
    [Arguments(2, null, 3, null, "--sa-cols:2;--sa-cols-sm:2;--sa-cols-md:3;--sa-cols-lg:3")]
    public async Task ProcessAsync_WhenColumnsAreSet_WritesColumnVariablesOnContent(
        int? columns,
        int? columnsSmall,
        int? columnsMedium,
        int? columnsLarge,
        string expected
    )
    {
        // Arrange
        var sut = new GridTagHelper
        {
            Columns = columns,
            ColumnsSmall = columnsSmall,
            ColumnsMedium = columnsMedium,
            ColumnsLarge = columnsLarge,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector(".sa-grid-content")?.GetAttribute("style"))
            .IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, null)]
    [Arguments(13, null)]
    [Arguments(null, 0)]
    [Arguments(null, 13)]
    public async Task ProcessAsync_WhenColumnsAreOutOfRange_ThrowsInvalidOperationException(
        int? columns,
        int? columnsSmall
    )
    {
        // Arrange
        var sut = new GridTagHelper { Columns = columns, ColumnsSmall = columnsSmall };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
