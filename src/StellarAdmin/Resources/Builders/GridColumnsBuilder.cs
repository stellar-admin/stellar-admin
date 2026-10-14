using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the number of columns at each breakpoint.
/// </summary>
/// <remarks>
///     A breakpoint without a count uses the count of the next smaller breakpoint.
/// </remarks>
public sealed class GridColumnsBuilder
{
    internal FormGridColumnDefinitions Columns { get; private set; }

    internal GridColumnsBuilder() { }

    /// <summary>
    ///     Sets the number of columns at the smallest width.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    public GridColumnsBuilder Base(int count)
    {
        Columns = Columns with { Base = ResponsiveGrid.ValidateColumns(count, nameof(count)) };

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the container is at least 56rem wide.
    /// </summary>
    public GridColumnsBuilder Large(int count)
    {
        Columns = Columns with { Large = ResponsiveGrid.ValidateColumns(count, nameof(count)) };

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the container is at least 40rem wide.
    /// </summary>
    public GridColumnsBuilder Medium(int count)
    {
        Columns = Columns with { Medium = ResponsiveGrid.ValidateColumns(count, nameof(count)) };

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the container is at least 30rem wide.
    /// </summary>
    public GridColumnsBuilder Small(int count)
    {
        Columns = Columns with { Small = ResponsiveGrid.ValidateColumns(count, nameof(count)) };

        return this;
    }

    internal static FormGridColumnDefinitions Build(Action<GridColumnsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new GridColumnsBuilder();
        configure(builder);

        return builder.Columns;
    }
}
