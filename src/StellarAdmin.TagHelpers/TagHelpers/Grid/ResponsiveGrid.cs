using System.Globalization;
using System.Text;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Resolves the column counts and spans of a responsive grid, and formats them as the inline
///     variables the StellarAdmin grids read.
/// </summary>
/// <remarks>
///     A breakpoint without a value uses the value of the next smaller breakpoint.
/// </remarks>
public static class ResponsiveGrid
{
    /// <summary>
    ///     A span that covers every column of its grid.
    /// </summary>
    public const int FullSpan = int.MaxValue;

    /// <summary>
    ///     The largest number of columns a grid can have.
    /// </summary>
    public const int MaxColumns = 12;

    /// <summary>
    ///     Returns the inline style that sets a grid's column counts.
    /// </summary>
    /// <returns>
    ///     The style, or <c>null</c> when every breakpoint has one column.
    /// </returns>
    public static string? ColumnsStyle(GridTiers columns) => Format("--sa-cols", columns);

    /// <summary>
    ///     Returns the column count at each breakpoint.
    /// </summary>
    /// <remarks>
    ///     A breakpoint without a count uses the count of the next smaller breakpoint; the smallest
    ///     defaults to 1.
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
    ///     Returns the span at each breakpoint, limited to the grid's column count at that
    ///     breakpoint.
    /// </summary>
    /// <remarks>
    ///     A breakpoint without a span uses the span of the next smaller breakpoint; the smallest
    ///     defaults to 1. Use <see cref="FullSpan" /> to span every column.
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
    ///     Returns the inline style that sets a grid's row counts.
    /// </summary>
    /// <returns>
    ///     The style, or <c>null</c> when every breakpoint has one row.
    /// </returns>
    public static string? RowsStyle(GridTiers rows) => Format("--sa-rows", rows);

    /// <summary>
    ///     Returns the inline style that sets a grid item's spans.
    /// </summary>
    /// <returns>
    ///     The style, or <c>null</c> when the item spans one column at every breakpoint.
    /// </returns>
    public static string? SpanStyle(GridTiers span) => Format("--sa-span", span);

    // A tier at 1 is left out, since the variables are registered with 1 as their initial value
    // and don't inherit.
    private static string? Format(string variable, GridTiers tiers)
    {
        var style = new StringBuilder();
        Append(style, variable, tiers.Base);
        Append(style, $"{variable}-sm", tiers.Small);
        Append(style, $"{variable}-md", tiers.Medium);
        Append(style, $"{variable}-lg", tiers.Large);

        return style.Length > 0 ? style.ToString() : null;
    }

    private static void Append(StringBuilder style, string variable, int value)
    {
        if (value == 1)
        {
            return;
        }

        if (style.Length > 0)
        {
            style.Append(';');
        }

        style.Append(variable).Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
    }

    private static int? ValidateColumns(int? value, string paramName)
    {
        if (value is { } count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 1, paramName);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(count, MaxColumns, paramName);
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
