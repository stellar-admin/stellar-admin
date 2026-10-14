using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Grid;

public class GridItemTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenInsideGrid_RendersItemWithUserClass()
    {
        // Arrange
        var sut = new GridItemTagHelper();
        var grid = new GridTagHelper { Columns = 3 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        var item = html.QuerySelector(".sa-grid-content > [data-slot=grid-item]");
        await Assert.That(item?.ClassName).IsEqualTo("sa-grid-item existing-class");
        await Assert.That(item?.HasAttribute("style")).IsFalse();
    }

    [Test]
    [Arguments("2", null, null, null, "--sa-span-sm:2;--sa-span-md:2;--sa-span-lg:2")]
    [Arguments("full", null, null, "2", "--sa-span-sm:2;--sa-span-md:3;--sa-span-lg:2")]
    [Arguments(null, null, "full", null, "--sa-span-md:3;--sa-span-lg:4")]
    [Arguments(null, "full", "1", null, "--sa-span-sm:2")]
    [Arguments("12", null, null, null, "--sa-span-sm:2;--sa-span-md:3;--sa-span-lg:4")]
    public async Task ProcessAsync_WhenSpansAreSet_ResolvesThemAgainstGridColumns(
        string? columnSpan,
        string? columnSpanSmall,
        string? columnSpanMedium,
        string? columnSpanLarge,
        string expected
    )
    {
        // Arrange
        var sut = new GridItemTagHelper
        {
            ColumnSpan = columnSpan,
            ColumnSpanSmall = columnSpanSmall,
            ColumnSpanMedium = columnSpanMedium,
            ColumnSpanLarge = columnSpanLarge,
        };
        var grid = new GridTagHelper
        {
            ColumnsSmall = 2,
            ColumnsMedium = 3,
            ColumnsLarge = 4,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=grid-item]")?.GetAttribute("style"))
            .IsEqualTo(expected);
    }

    [Test]
    [Arguments("1", null, null, null, "--sa-start-sm:1;--sa-start-md:1;--sa-start-lg:1")]
    [Arguments("2", null, "auto", null, "--sa-start-sm:2")]
    [Arguments(null, null, "3", "AUTO", "--sa-start-md:3")]
    [Arguments(null, null, null, "4", "--sa-start-lg:4")]
    [Arguments("5", null, null, null, null)]
    public async Task ProcessAsync_WhenStartsAreSet_ResolvesThemAgainstGridColumns(
        string? columnStart,
        string? columnStartSmall,
        string? columnStartMedium,
        string? columnStartLarge,
        string? expected
    )
    {
        // Arrange
        var sut = new GridItemTagHelper
        {
            ColumnStart = columnStart,
            ColumnStartSmall = columnStartSmall,
            ColumnStartMedium = columnStartMedium,
            ColumnStartLarge = columnStartLarge,
        };
        var grid = new GridTagHelper
        {
            ColumnsSmall = 2,
            ColumnsMedium = 3,
            ColumnsLarge = 4,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=grid-item]")?.GetAttribute("style"))
            .IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_WhenSpanAndStartAreSet_WritesSpanThenStart()
    {
        // Arrange
        var sut = new GridItemTagHelper { ColumnSpan = "2", ColumnStartLarge = "3" };
        var grid = new GridTagHelper { Columns = 2, ColumnsLarge = 4 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=grid-item]")?.GetAttribute("style"))
            .IsEqualTo("--sa-span-md:2;--sa-span-lg:2;--sa-start-lg:3");
    }

    [Test]
    public async Task ProcessAsync_WhenUserSuppliesStyle_KeepsItBeforeSpan()
    {
        // Arrange
        var sut = new GridItemTagHelper { ColumnSpan = "full" };
        var grid = new GridTagHelper { Columns = 2 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext =>
                TagHelperRenderer.RenderChildAsync(
                    sut,
                    gridContext,
                    outputAttributes: [new("style", "order: 2")]
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=grid-item]")?.GetAttribute("style"))
            .IsEqualTo("order: 2; --sa-span-md:2;--sa-span-lg:2");
    }

    [Test]
    public async Task ProcessAsync_WhenOutsideGrid_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new GridItemTagHelper();

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WhenInsideDetails_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new GridItemTagHelper();
        var details = new DetailsTagHelper();

        // Act
        Func<Task> act = () =>
            TagHelperRenderer.RenderAsync(
                details,
                detailsContext => TagHelperRenderer.RenderChildAsync(sut, detailsContext)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("0")]
    [Arguments("full-width")]
    public async Task ProcessAsync_WhenSpanIsInvalid_ThrowsInvalidOperationException(
        string columnSpan
    )
    {
        // Arrange
        var sut = new GridItemTagHelper { ColumnSpanLarge = columnSpan };
        var grid = new GridTagHelper { Columns = 2 };

        // Act
        Func<Task> act = () =>
            TagHelperRenderer.RenderAsync(
                grid,
                gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("0")]
    [Arguments("13")]
    [Arguments("first")]
    public async Task ProcessAsync_WhenStartIsInvalid_ThrowsInvalidOperationException(
        string columnStart
    )
    {
        // Arrange
        var sut = new GridItemTagHelper { ColumnStartMedium = columnStart };
        var grid = new GridTagHelper { Columns = 2 };

        // Act
        Func<Task> act = () =>
            TagHelperRenderer.RenderAsync(
                grid,
                gridContext => TagHelperRenderer.RenderChildAsync(sut, gridContext)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
