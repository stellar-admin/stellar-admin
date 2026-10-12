using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the column a form item starts at, at each breakpoint.
/// </summary>
/// <remarks>
///     A breakpoint without a start uses the start of the next smaller breakpoint. A start is
///     ignored where the grid has one column or the item would extend past the last column.
/// </remarks>
public sealed class ColumnStartBuilder
{
    internal FormColumnStartDefinitions Start { get; private set; }

    internal ColumnStartBuilder() { }

    /// <summary>
    ///     Sets the start at the smallest width.
    /// </summary>
    /// <remarks>
    ///     Defaults to following the item before it.
    /// </remarks>
    public ColumnStartBuilder Base(int start)
    {
        Start = Start with { Base = ResponsiveGrid.ValidateColumns(start, nameof(start)) };

        return this;
    }

    /// <summary>
    ///     Sets the start when the form is at least 56rem wide.
    /// </summary>
    public ColumnStartBuilder Large(int start)
    {
        Start = Start with { Large = ResponsiveGrid.ValidateColumns(start, nameof(start)) };

        return this;
    }

    /// <summary>
    ///     Follows the item before it when the form is at least 56rem wide.
    /// </summary>
    public ColumnStartBuilder LargeAuto()
    {
        Start = Start with { Large = ResponsiveGrid.AutoStart };

        return this;
    }

    /// <summary>
    ///     Sets the start when the form is at least 40rem wide.
    /// </summary>
    public ColumnStartBuilder Medium(int start)
    {
        Start = Start with { Medium = ResponsiveGrid.ValidateColumns(start, nameof(start)) };

        return this;
    }

    /// <summary>
    ///     Follows the item before it when the form is at least 40rem wide.
    /// </summary>
    public ColumnStartBuilder MediumAuto()
    {
        Start = Start with { Medium = ResponsiveGrid.AutoStart };

        return this;
    }

    /// <summary>
    ///     Sets the start when the form is at least 30rem wide.
    /// </summary>
    public ColumnStartBuilder Small(int start)
    {
        Start = Start with { Small = ResponsiveGrid.ValidateColumns(start, nameof(start)) };

        return this;
    }

    /// <summary>
    ///     Follows the item before it when the form is at least 30rem wide.
    /// </summary>
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

    internal static FormColumnStartDefinitions FromStart(int start) =>
        new(Base: ResponsiveGrid.ValidateColumns(start, nameof(start)));
}
