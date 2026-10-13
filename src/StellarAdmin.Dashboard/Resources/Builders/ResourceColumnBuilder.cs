using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Displays;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures an index column.
/// </summary>
public sealed class ResourceColumnBuilder<TResource>
{
    private readonly List<Action<DataGridColumnOptions>> _configuration = [];
    private readonly LambdaExpression _field;

    /// <summary>
    ///     The text a cell shows when its value is empty. Defaults to the property's
    ///     <c>[DisplayFormat(NullDisplayText)]</c>, then an empty cell.
    /// </summary>
    public string? EmptyText
    {
        set => _configuration.Add(options => options.EmptyText = value);
    }

    /// <summary>
    ///     The column title.
    /// </summary>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    internal ResourceColumnBuilder(LambdaExpression field) => _field = field;

    /// <summary>
    ///     Enables sorting by the column's field.
    /// </summary>
    public ResourceColumnBuilder<TResource> Sortable()
    {
        _configuration.Add(options =>
        {
            options.Sortable = true;
            options.SortExpression = null;
        });

        return this;
    }

    /// <summary>
    ///     Enables sorting by the selected expression.
    /// </summary>
    public ResourceColumnBuilder<TResource> Sortable<TSort>(
        Expression<Func<TResource, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _configuration.Add(options =>
        {
            options.Sortable = true;
            options.SortExpression = selector;
        });

        return this;
    }

    /// <summary>
    ///     Uses and configures a display for the column's cells.
    /// </summary>
    /// <remarks>
    ///     Without a display, the cells render the grid display template their property's
    ///     <c>[UIHint]</c> names, or the default display for its type. Displays with a handler
    ///     are not supported in columns.
    /// </remarks>
    public ResourceColumnBuilder<TResource> UseDisplay<TDisplay>(Action<TDisplay> configure)
        where TDisplay : FieldDisplay, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        // A handler would prepare its data once for every row, so columns leave them out for now
        if (typeof(IFieldDisplay).IsAssignableFrom(typeof(TDisplay)))
        {
            throw new NotSupportedException(
                $"{typeof(TDisplay).Name} has a handler, which columns do not support."
            );
        }

        _configuration.Add(options =>
        {
            var display = new TDisplay();
            configure(display);
            options.Display = display;
            options.UsesBuiltInRendering = false;
        });

        return this;
    }

    /// <summary>
    ///     Uses a display for the column's cells.
    /// </summary>
    public ResourceColumnBuilder<TResource> UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    /// <summary>
    ///     Renders the cells from the property's metadata instead of a display.
    /// </summary>
    /// <remarks>
    ///     The value is formatted with the property's <c>[DisplayFormat]</c>.
    /// </remarks>
    public ResourceColumnBuilder<TResource> UseBuiltInRendering()
    {
        _configuration.Add(options =>
        {
            options.Display = null;
            options.UsesBuiltInRendering = true;
        });

        return this;
    }

    internal DataGridColumnOptions Build()
    {
        var options = new DataGridColumnOptions(
            _field,
            ResourcePropertyPath.GetProperties(_field) is { } properties
                ? ResourcePropertyPath.GetName(properties)
                : null
        );
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
