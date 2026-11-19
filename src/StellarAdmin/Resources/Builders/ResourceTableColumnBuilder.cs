using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a column of a table on a resource's index page.
/// </summary>
/// <typeparam name="TItem">
///     The type of the table's rows: the resource, or the items a handler loads.
/// </typeparam>
public sealed class ResourceTableColumnBuilder<TItem>
{
    private readonly List<Action<DataGridColumnOptions>> _configuration = [];
    private readonly LambdaExpression _field;
    private readonly DataGridColumnGroupOptions? _group;
    private readonly ConfigurationSource? _source = ConfigurationSource.Capture();

    /// <summary>
    ///     Sets how the column aligns its head and cells.
    /// </summary>
    /// <value>
    ///     The default is <see cref="DataGridColumnAlign.End" /> for a column whose display is
    ///     <see cref="NumberDisplay" />, and <see cref="DataGridColumnAlign.Start" /> otherwise.
    /// </value>
    public DataGridColumnAlign Align
    {
        set => _configuration.Add(options => options.Align = value);
    }

    /// <summary>
    ///     Sets the text a cell shows when its value is empty.
    /// </summary>
    /// <value>
    ///     The default is the property's <c>[DisplayFormat(NullDisplayText)]</c>, or an empty cell
    ///     when the property doesn't set one.
    /// </value>
    /// <remarks>
    ///     Not used with a display that renders empty values itself, such as
    ///     <see cref="ImageDisplay" />.
    /// </remarks>
    public string? EmptyText
    {
        set => _configuration.Add(options => options.EmptyText = value);
    }

    /// <summary>
    ///     Sets the column title.
    /// </summary>
    /// <value>
    ///     The default is the property's display name. A column that computes its value has no
    ///     property, so it requires a title, which also names the column in sort links.
    /// </value>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    internal ResourceTableColumnBuilder(
        LambdaExpression field,
        DataGridColumnGroupOptions? group = null
    )
    {
        _field = field;
        _group = group;
    }

    /// <summary>
    ///     Sets the form field the column edits in an inline row.
    /// </summary>
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">An expression that selects the field's property path.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Use it when the column shows a different property than the field binds. By default,
    ///     the column edits the field with the same property path. An expression that doesn't
    ///     select a property path is a configuration problem, and the call is ignored.
    /// </remarks>
    public ResourceTableColumnBuilder<TItem> EditsField<TProperty>(
        Expression<Func<TItem, TProperty>> field
    )
    {
        ArgumentNullException.ThrowIfNull(field);
        if (ResourcePropertyPath.GetProperties(field) is not { } properties)
        {
            _configuration.Add(options =>
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.InvalidSelector,
                    $"The `{options.FieldName ?? options.Title}` column doesn't edit `{field}` because it isn't a property path. It edits the field with its own property path instead.",
                    "Pass `EditsField` a property path of the resource.",
                    options.Source
                )
            );

            return this;
        }

        return EditsField(ResourcePropertyPath.GetName(properties));
    }

    /// <summary>
    ///     Sets the form field the column edits in an inline row.
    /// </summary>
    /// <param name="fieldName">
    ///     The field's property path, such as <c>CountryId</c>. It can name a property of a
    ///     separate edit or create model.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="fieldName" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Use it when the column shows a different property than the field binds. By default,
    ///     the column edits the field with the same property path. An empty or white space name
    ///     is a configuration problem, and the call is ignored.
    /// </remarks>
    public ResourceTableColumnBuilder<TItem> EditsField(string fieldName)
    {
        ArgumentNullException.ThrowIfNull(fieldName);

        _configuration.Add(options =>
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.InvalidValue,
                    $"The `{options.FieldName ?? options.Title}` column edits the field with its own property path because the field name it was given is blank.",
                    "Pass `EditsField` the name of a field.",
                    options.Source
                );

                return;
            }

            options.EditsFieldName = fieldName;
        });

        return this;
    }

    /// <summary>
    ///     Lets users sort by the column's property.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A column that computes its value fails at startup with this method; sort it with
    ///     <see cref="Sortable{TSort}(Expression{Func{TItem, TSort}})" /> instead.
    /// </remarks>
    public ResourceTableColumnBuilder<TItem> Sortable()
    {
        _configuration.Add(options =>
        {
            options.Sortable = true;
            options.SortExpression = null;
        });

        return this;
    }

    /// <summary>
    ///     Lets users sort the column by a different expression.
    /// </summary>
    /// <typeparam name="TSort">The type of the value to sort by.</typeparam>
    /// <param name="selector">An expression that selects the value to sort by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="selector" /> is <see langword="null" />.
    /// </exception>
    public ResourceTableColumnBuilder<TItem> Sortable<TSort>(
        Expression<Func<TItem, TSort>> selector
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
    ///     Renders the column's cells with a display and configures it.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <param name="configure">A delegate that configures the display.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Without a display, the cells render the compact display template their property's
    ///     <c>[UIHint]</c> names, or the default display for its type. Columns don't support
    ///     displays with a handler: one is a configuration problem, and the column keeps its
    ///     default display.
    /// </remarks>
    public ResourceTableColumnBuilder<TItem> UseDisplay<TDisplay>(Action<TDisplay> configure)
        where TDisplay : FieldDisplay, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        // A handler would prepare its data once for every row, so columns leave them out for now
        if (typeof(IFieldDisplay).IsAssignableFrom(typeof(TDisplay)))
        {
            _configuration.Add(options =>
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.ColumnDisplayWithHandler,
                    $"The `{options.FieldName ?? options.Title}` column doesn't use `{typeof(TDisplay).Name}` because it has a handler, which columns don't support. It uses its default display.",
                    "Use a display without a handler for the column.",
                    options.Source
                )
            );

            return this;
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
    ///     Renders the column's cells with a display.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Columns don't support displays with a handler: one is a configuration problem, and the
    ///     column keeps its default display.
    /// </remarks>
    public ResourceTableColumnBuilder<TItem> UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    /// <summary>
    ///     Renders the cells from the property's metadata instead of a display.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The value is formatted with the property's <c>[DisplayFormat]</c>.
    /// </remarks>
    public ResourceTableColumnBuilder<TItem> UseBuiltInRendering()
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
        )
        {
            Group = _group,
            Source = _source,
        };
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
