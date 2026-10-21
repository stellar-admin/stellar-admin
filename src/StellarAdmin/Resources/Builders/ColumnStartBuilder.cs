using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the column a form item starts at, at each breakpoint.
/// </summary>
/// <remarks>
///     A breakpoint without a start uses the start of the next smaller breakpoint. A start is
///     ignored where the grid has one column or the item would extend past the last column.
///     Breakpoints are measured against the width of the item's grid.
/// </remarks>
public sealed class ColumnStartBuilder
{
    internal FormColumnStartDefinitions Start { get; private set; }

    internal ColumnStartBuilder() { }

    /// <summary>
    ///     Sets the start at the smallest width.
    /// </summary>
    /// <param name="start">The column number, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     By default, the item follows the item before it.
    /// </remarks>
    public ColumnStartBuilder Base(int start)
    {
        Start = Start with { Base = Requested(start) };

        return this;
    }

    /// <summary>
    ///     Sets the start when the grid is at least 56rem wide.
    /// </summary>
    /// <param name="start">The column number, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ColumnStartBuilder Large(int start)
    {
        Start = Start with { Large = Requested(start) };

        return this;
    }

    /// <summary>
    ///     Places the item after the item before it when the grid is at least 56rem wide.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ColumnStartBuilder LargeAuto()
    {
        Start = Start with { Large = ResponsiveGrid.AutoStart };

        return this;
    }

    /// <summary>
    ///     Sets the start when the grid is at least 40rem wide.
    /// </summary>
    /// <param name="start">The column number, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ColumnStartBuilder Medium(int start)
    {
        Start = Start with { Medium = Requested(start) };

        return this;
    }

    /// <summary>
    ///     Places the item after the item before it when the grid is at least 40rem wide.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ColumnStartBuilder MediumAuto()
    {
        Start = Start with { Medium = ResponsiveGrid.AutoStart };

        return this;
    }

    /// <summary>
    ///     Sets the start when the grid is at least 30rem wide.
    /// </summary>
    /// <param name="start">The column number, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ColumnStartBuilder Small(int start)
    {
        Start = Start with { Small = Requested(start) };

        return this;
    }

    /// <summary>
    ///     Places the item after the item before it when the grid is at least 30rem wide.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ColumnStartBuilder SmallAuto()
    {
        Start = Start with { Small = ResponsiveGrid.AutoStart };

        return this;
    }

    internal static FormColumnStartDefinitions Build(Action<ColumnStartBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ColumnStartBuilder();
        configure(builder);

        return builder.Start;
    }

    internal static FormColumnStartDefinitions FromStart(int start) => new(Base: Requested(start));

    // A requested start of 0 is out of range, but 0 also marks an automatic start, so it is kept as another
    // out-of-range number for the grid layout check to report
    private static int Requested(int start) => start == ResponsiveGrid.AutoStart ? -1 : start;
}
