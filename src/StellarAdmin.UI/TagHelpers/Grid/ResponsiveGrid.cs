using System.Globalization;
using System.Text;

namespace StellarAdmin.UI;

/// <summary>
///     Resolves the column counts, spans and starts of a responsive grid, and formats them as
///     inline styles.
/// </summary>
/// <remarks>
///     A breakpoint without a value uses the value of the next smaller breakpoint.
/// </remarks>
public static class ResponsiveGrid
{
    /// <summary>
    ///     Represents a start that places the item after the item before it.
    /// </summary>
    public const int AutoStart = 0;

    /// <summary>
    ///     Represents a span that covers every column of its grid.
    /// </summary>
    public const int FullSpan = int.MaxValue;

    /// <summary>
    ///     Represents the largest number of columns a grid can have.
    /// </summary>
    public const int MaxColumns = 12;

    /// <summary>
    ///     Returns the inline style that sets a grid's column counts.
    /// </summary>
    /// <param name="columns">The column count at each breakpoint.</param>
    /// <returns>
    ///     The style, or <see langword="null" /> when every breakpoint has one column.
    /// </returns>
    public static string? ColumnsStyle(GridTiers columns) => Format("--sa-cols", columns);

    /// <summary>
    ///     Returns the column count at each breakpoint.
    /// </summary>
    /// <param name="base">The column count at the smallest width.</param>
    /// <param name="small">The column count when the grid is at least 30rem wide.</param>
    /// <param name="medium">The column count when the grid is at least 40rem wide.</param>
    /// <param name="large">The column count when the grid is at least 56rem wide.</param>
    /// <returns>The column count at each breakpoint.</returns>
    /// <remarks>
    ///     A <see langword="null" /> count uses the count of the next smaller breakpoint. The
    ///     smallest uses 1.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     A count is less than 1 or greater than <see cref="MaxColumns" />.
    /// </exception>
    public static GridTiers ResolveColumns(
        int? @base = null,
        int? small = null,
        int? medium = null,
        int? large = null
    )
    {
        var baseCount = ValidateColumns(@base, nameof(@base)) ?? 1;
        var smallCount = ValidateColumns(small, nameof(small)) ?? baseCount;
        var mediumCount = ValidateColumns(medium, nameof(medium)) ?? smallCount;
        var largeCount = ValidateColumns(large, nameof(large)) ?? mediumCount;

        return new(baseCount, smallCount, mediumCount, largeCount);
    }

    /// <summary>
    ///     Returns the row count at each breakpoint that fills each column before the next one
    ///     starts.
    /// </summary>
    /// <param name="columns">The column count at each breakpoint.</param>
    /// <param name="itemCount">The number of items in the grid.</param>
    /// <returns>The row count at each breakpoint, at least 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="itemCount" /> is negative.
    /// </exception>
    public static GridTiers ResolveRows(GridTiers columns, int itemCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(itemCount);

        return new(
            Rows(itemCount, columns.Base),
            Rows(itemCount, columns.Small),
            Rows(itemCount, columns.Medium),
            Rows(itemCount, columns.Large)
        );
    }

    /// <summary>
    ///     Returns an item's span at each breakpoint, limited to the grid's column count there.
    /// </summary>
    /// <param name="columns">The grid's column count at each breakpoint.</param>
    /// <param name="base">The span at the smallest width.</param>
    /// <param name="small">The span when the grid is at least 30rem wide.</param>
    /// <param name="medium">The span when the grid is at least 40rem wide.</param>
    /// <param name="large">The span when the grid is at least 56rem wide.</param>
    /// <returns>The span at each breakpoint.</returns>
    /// <remarks>
    ///     A <see langword="null" /> span uses the span of the next smaller breakpoint. The
    ///     smallest uses 1. Use <see cref="FullSpan" /> to span every column.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     A span is less than 1.
    /// </exception>
    public static GridTiers ResolveSpan(
        GridTiers columns,
        int? @base = null,
        int? small = null,
        int? medium = null,
        int? large = null
    )
    {
        var baseSpan = ValidateSpan(@base, nameof(@base)) ?? 1;
        var smallSpan = ValidateSpan(small, nameof(small)) ?? baseSpan;
        var mediumSpan = ValidateSpan(medium, nameof(medium)) ?? smallSpan;
        var largeSpan = ValidateSpan(large, nameof(large)) ?? mediumSpan;

        return new(
            Math.Min(baseSpan, columns.Base),
            Math.Min(smallSpan, columns.Small),
            Math.Min(mediumSpan, columns.Medium),
            Math.Min(largeSpan, columns.Large)
        );
    }

    /// <summary>
    ///     Returns the column an item starts at, for each breakpoint.
    /// </summary>
    /// <param name="columns">The grid's column count at each breakpoint.</param>
    /// <param name="span">The item's span at each breakpoint.</param>
    /// <param name="base">The start at the smallest width.</param>
    /// <param name="small">The start when the grid is at least 30rem wide.</param>
    /// <param name="medium">The start when the grid is at least 40rem wide.</param>
    /// <param name="large">The start when the grid is at least 56rem wide.</param>
    /// <returns>The start at each breakpoint.</returns>
    /// <remarks>
    ///     A <see langword="null" /> start uses the start of the next smaller breakpoint. The
    ///     smallest uses <see cref="AutoStart" />. A start becomes <see cref="AutoStart" /> at a
    ///     breakpoint where the grid has one column or the item would extend past the last column.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     A start is less than <see cref="AutoStart" /> or greater than <see cref="MaxColumns" />.
    /// </exception>
    public static GridTiers ResolveStart(
        GridTiers columns,
        GridTiers span,
        int? @base = null,
        int? small = null,
        int? medium = null,
        int? large = null
    )
    {
        var baseStart = ValidateStart(@base, nameof(@base)) ?? AutoStart;
        var smallStart = ValidateStart(small, nameof(small)) ?? baseStart;
        var mediumStart = ValidateStart(medium, nameof(medium)) ?? smallStart;
        var largeStart = ValidateStart(large, nameof(large)) ?? mediumStart;

        return new(
            FitStart(baseStart, columns.Base, span.Base),
            FitStart(smallStart, columns.Small, span.Small),
            FitStart(mediumStart, columns.Medium, span.Medium),
            FitStart(largeStart, columns.Large, span.Large)
        );
    }

    /// <summary>
    ///     Returns the inline style that sets a grid's row counts.
    /// </summary>
    /// <param name="rows">The row count at each breakpoint.</param>
    /// <returns>
    ///     The style, or <see langword="null" /> when every breakpoint has one row.
    /// </returns>
    public static string? RowsStyle(GridTiers rows) => Format("--sa-rows", rows);

    /// <summary>
    ///     Returns the inline style that sets a grid item's spans.
    /// </summary>
    /// <param name="span">The span at each breakpoint.</param>
    /// <returns>
    ///     The style, or <see langword="null" /> when the item spans one column at every
    ///     breakpoint.
    /// </returns>
    public static string? SpanStyle(GridTiers span) => Format("--sa-span", span);

    /// <summary>
    ///     Returns the inline style that sets a grid item's starts.
    /// </summary>
    /// <param name="start">The start at each breakpoint.</param>
    /// <returns>
    ///     The style, or <see langword="null" /> when the item has no start at any breakpoint.
    /// </returns>
    public static string? StartStyle(GridTiers start) =>
        Format("--sa-start", start, omitted: AutoStart);

    /// <summary>
    ///     Checks that a column count, span or start is from 1 to <see cref="MaxColumns" />.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter the value came from.</param>
    /// <returns>The value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="value" /> is less than 1 or greater than <see cref="MaxColumns" />.
    /// </exception>
    public static int ValidateColumns(int value, string paramName)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxColumns, paramName);

        return value;
    }

    private static int FitStart(int start, int columns, int span) =>
        columns == 1 || start + span - 1 > columns ? AutoStart : start;

    // A tier at the variable's initial value is left out, since the variables are registered with
    // it and don't inherit: 1 for counts and spans, auto for starts.
    private static string? Format(string variable, GridTiers tiers, int omitted = 1)
    {
        var style = new StringBuilder();
        Append(style, variable, tiers.Base, omitted);
        Append(style, $"{variable}-sm", tiers.Small, omitted);
        Append(style, $"{variable}-md", tiers.Medium, omitted);
        Append(style, $"{variable}-lg", tiers.Large, omitted);

        return style.Length > 0 ? style.ToString() : null;
    }

    private static void Append(StringBuilder style, string variable, int value, int omitted)
    {
        if (value == omitted)
        {
            return;
        }

        if (style.Length > 0)
        {
            style.Append(';');
        }

        style.Append(variable).Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
    }

    private static int Rows(int itemCount, int columns) =>
        Math.Max(1, (itemCount + columns - 1) / columns);

    private static int? ValidateColumns(int? value, string paramName) =>
        value is { } count ? ValidateColumns(count, paramName) : null;

    private static int? ValidateStart(int? value, string paramName)
    {
        if (value is { } start)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(start, AutoStart, paramName);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, MaxColumns, paramName);
        }

        return value;
    }

    private static int? ValidateSpan(int? value, string paramName)
    {
        if (value is { } span)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(span, 1, paramName);
        }

        return value;
    }
}
