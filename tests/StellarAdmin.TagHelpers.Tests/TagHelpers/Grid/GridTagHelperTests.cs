using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Grid;

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
        await Assert.That(html.QuerySelector(".sa-grid-content")?.HasAttribute("style")).IsFalse();
    }

    [Test]
    [Arguments(3, null, null, null, null, "--sa-cols-md:3;--sa-cols-lg:3")]
    [Arguments(3, null, 2, null, 4, "--sa-cols-sm:2;--sa-cols-md:3;--sa-cols-lg:4")]
    [Arguments(
        null,
        2,
        null,
        null,
        null,
        "--sa-cols:2;--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:2"
    )]
    [Arguments(null, null, null, 2, null, "--sa-cols-md:2;--sa-cols-lg:2")]
    public async Task ProcessAsync_WhenColumnsAreSet_WritesColumnVariablesOnContent(
        int? columns,
        int? columnsBase,
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
            ColumnsBase = columnsBase,
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
        int? columnsBase
    )
    {
        // Arrange
        var sut = new GridTagHelper { Columns = columns, ColumnsBase = columnsBase };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WhenColumnsAndColumnsMediumAreSet_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new GridTagHelper { Columns = 2, ColumnsMedium = 3 };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
