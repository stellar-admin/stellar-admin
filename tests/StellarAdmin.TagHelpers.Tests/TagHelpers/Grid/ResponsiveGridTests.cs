namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Grid;

public class ResponsiveGridTests
{
    [Test]
    public async Task ResolveColumns_WithNoValues_UsesOneColumn()
    {
        // Arrange

        // Act
        var columns = ResponsiveGrid.ResolveColumns();

        // Assert
        await Assert.That(columns).IsEqualTo(new GridTiers(1, 1, 1, 1));
    }

    [Test]
    public async Task ResolveColumns_WithUnsetBreakpoints_InheritsFromNextSmallerBreakpoint()
    {
        // Arrange

        // Act
        var columns = ResponsiveGrid.ResolveColumns(small: 2, large: 4);

        // Assert
        await Assert.That(columns).IsEqualTo(new GridTiers(1, 2, 2, 4));
    }

    [Test]
    [Arguments(0)]
    [Arguments(13)]
    public async Task ResolveColumns_WithCountOutOfRange_ThrowsArgumentOutOfRangeException(
        int count
    )
    {
        // Arrange

        // Act
        Action act = () => ResponsiveGrid.ResolveColumns(medium: count);

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ResolveSpan_WithNoValues_SpansOneColumn()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);

        // Act
        var span = ResponsiveGrid.ResolveSpan(columns);

        // Assert
        await Assert.That(span).IsEqualTo(new GridTiers(1, 1, 1, 1));
    }

    [Test]
    public async Task ResolveSpan_WithFullSpan_SpansAllColumnsAtEachBreakpoint()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);

        // Act
        var span = ResponsiveGrid.ResolveSpan(columns, ResponsiveGrid.FullSpan);

        // Assert
        await Assert.That(span).IsEqualTo(new GridTiers(1, 2, 3, 4));
    }

    [Test]
    public async Task ResolveSpan_WithSpanWiderThanGrid_InheritsRequestedSpanAndLimitsEachBreakpoint()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 4, 4);

        // Act
        var span = ResponsiveGrid.ResolveSpan(columns, 3, large: 2);

        // Assert
        await Assert.That(span).IsEqualTo(new GridTiers(1, 2, 3, 2));
    }

    [Test]
    public async Task ResolveSpan_WithSpanLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);

        // Act
        Action act = () => ResponsiveGrid.ResolveSpan(columns, small: 0);

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ColumnsStyle_WithValuesAboveOne_WritesOnlyThoseBreakpoints()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 2, 4);

        // Act
        var style = ResponsiveGrid.ColumnsStyle(columns);

        // Assert
        await Assert.That(style).IsEqualTo("--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:4");
    }

    [Test]
    public async Task SpanStyle_WithOneAtEveryBreakpoint_ReturnsNull()
    {
        // Arrange
        var span = new GridTiers(1, 1, 1, 1);

        // Act
        var style = ResponsiveGrid.SpanStyle(span);

        // Assert
        await Assert.That(style).IsNull();
    }

    [Test]
    public async Task RowsStyle_WithEveryBreakpointAboveOne_WritesEveryBreakpoint()
    {
        // Arrange
        var rows = new GridTiers(4, 3, 2, 2);

        // Act
        var style = ResponsiveGrid.RowsStyle(rows);

        // Assert
        await Assert
            .That(style)
            .IsEqualTo("--sa-rows:4;--sa-rows-sm:3;--sa-rows-md:2;--sa-rows-lg:2");
    }
}
