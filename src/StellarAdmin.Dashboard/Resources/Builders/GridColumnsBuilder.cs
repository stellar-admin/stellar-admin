using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

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
    public GridColumnsBuilder Default(int count)
    {
        Columns = Columns with { Default = FormGrid.ValidateColumns(count, nameof(count)) };

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the container is at least 56rem wide.
    /// </summary>
    public GridColumnsBuilder Large(int count)
    {
        Columns = Columns with { Large = FormGrid.ValidateColumns(count, nameof(count)) };

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the container is at least 40rem wide.
    /// </summary>
    public GridColumnsBuilder Medium(int count)
    {
        Columns = Columns with { Medium = FormGrid.ValidateColumns(count, nameof(count)) };

        return this;
    }

    /// <summary>
    ///     Sets the number of columns when the container is at least 30rem wide.
    /// </summary>
    public GridColumnsBuilder Small(int count)
    {
        Columns = Columns with { Small = FormGrid.ValidateColumns(count, nameof(count)) };

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
