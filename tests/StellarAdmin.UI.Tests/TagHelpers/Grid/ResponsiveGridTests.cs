namespace StellarAdmin.UI.Tests.TagHelpers.Grid;

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
    public async Task ResolveRows_WithItems_FillsEachColumnBeforeTheNext()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 2, 3);

        // Act
        var rows = ResponsiveGrid.ResolveRows(columns, 7);

        // Assert
        await Assert.That(rows).IsEqualTo(new GridTiers(7, 4, 4, 3));
    }

    [Test]
    public async Task ResolveRows_WithoutItems_UsesOneRow()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);

        // Act
        var rows = ResponsiveGrid.ResolveRows(columns, 0);

        // Assert
        await Assert.That(rows).IsEqualTo(new GridTiers(1, 1, 1, 1));
    }

    [Test]
    public async Task ResolveRows_WithNegativeItemCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);

        // Act
        Action act = () => ResponsiveGrid.ResolveRows(columns, -1);

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
    public async Task ResolveStart_WithNoValues_UsesAutoStart()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);
        var span = new GridTiers(1, 1, 1, 1);

        // Act
        var start = ResponsiveGrid.ResolveStart(columns, span);

        // Assert
        await Assert.That(start).IsEqualTo(new GridTiers(0, 0, 0, 0));
    }

    [Test]
    public async Task ResolveStart_WithUnsetBreakpoints_InheritsFromNextSmallerBreakpoint()
    {
        // Arrange
        var columns = new GridTiers(2, 2, 3, 4);
        var span = new GridTiers(1, 1, 1, 1);

        // Act
        var start = ResponsiveGrid.ResolveStart(columns, span, 2, large: ResponsiveGrid.AutoStart);

        // Assert
        await Assert.That(start).IsEqualTo(new GridTiers(2, 2, 2, 0));
    }

    [Test]
    public async Task ResolveStart_WithOneColumn_DropsStart()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 2, 2);
        var span = new GridTiers(1, 1, 1, 1);

        // Act
        var start = ResponsiveGrid.ResolveStart(columns, span, 1);

        // Assert
        await Assert.That(start).IsEqualTo(new GridTiers(0, 1, 1, 1));
    }

    [Test]
    public async Task ResolveStart_WhenItemWouldExtendPastLastColumn_DropsStartAtThatBreakpoint()
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);
        var span = new GridTiers(1, 2, 2, 2);

        // Act
        var start = ResponsiveGrid.ResolveStart(columns, span, small: 2);

        // Assert
        await Assert.That(start).IsEqualTo(new GridTiers(0, 0, 2, 2));
    }

    [Test]
    [Arguments(-1)]
    [Arguments(13)]
    public async Task ResolveStart_WithStartOutOfRange_ThrowsArgumentOutOfRangeException(int value)
    {
        // Arrange
        var columns = new GridTiers(1, 2, 3, 4);
        var span = new GridTiers(1, 1, 1, 1);

        // Act
        Action act = () => ResponsiveGrid.ResolveStart(columns, span, medium: value);

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
    public async Task StartStyle_WithAutoStartAtEveryBreakpoint_ReturnsNull()
    {
        // Arrange
        var start = new GridTiers(0, 0, 0, 0);

        // Act
        var style = ResponsiveGrid.StartStyle(start);

        // Assert
        await Assert.That(style).IsNull();
    }

    [Test]
    public async Task StartStyle_WithStarts_WritesEveryBreakpointWithAStartIncludingOne()
    {
        // Arrange
        var start = new GridTiers(0, 1, 2, 0);

        // Act
        var style = ResponsiveGrid.StartStyle(start);

        // Assert
        await Assert.That(style).IsEqualTo("--sa-start-sm:1;--sa-start-md:2");
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

    [Test]
    [Arguments(1)]
    [Arguments(12)]
    public async Task ValidateColumns_WithValueInRange_ReturnsValue(int value)
    {
        // Arrange

        // Act
        var result = ResponsiveGrid.ValidateColumns(value, nameof(value));

        // Assert
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    [Arguments(0)]
    [Arguments(13)]
    public async Task ValidateColumns_WithValueOutOfRange_ThrowsArgumentOutOfRangeException(
        int value
    )
    {
        // Arrange

        // Act
        Action act = () => ResponsiveGrid.ValidateColumns(value, nameof(value));

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }
}
