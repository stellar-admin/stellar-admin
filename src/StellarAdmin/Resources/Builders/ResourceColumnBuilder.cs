using System.Linq.Expressions;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a column of the index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceColumnBuilder<TResource>
{
    private readonly List<Action<DataGridColumnOptions>> _configuration = [];
    private readonly LambdaExpression _field;

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

    internal ResourceColumnBuilder(LambdaExpression field) => _field = field;

    /// <summary>
    ///     Sets the form field the column edits in an inline row.
    /// </summary>
    /// <typeparam name="TProperty">The type of the field's property.</typeparam>
    /// <param name="field">An expression that selects the field's property path.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> doesn't select a property path.
    /// </exception>
    /// <remarks>
    ///     Use it when the column shows a different property than the field binds. By default,
    ///     the column edits the field with the same property path.
    /// </remarks>
    public ResourceColumnBuilder<TResource> EditsField<TProperty>(
        Expression<Func<TResource, TProperty>> field
    )
    {
        ArgumentNullException.ThrowIfNull(field);
        if (ResourcePropertyPath.GetProperties(field) is not { } properties)
        {
            throw new ArgumentException("Select a property path.", nameof(field));
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="fieldName" /> is empty or consists only of white space.
    /// </exception>
    /// <remarks>
    ///     Use it when the column shows a different property than the field binds. By default,
    ///     the column edits the field with the same property path.
    /// </remarks>
    public ResourceColumnBuilder<TResource> EditsField(string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        _configuration.Add(options => options.EditsFieldName = fieldName);

        return this;
    }

    /// <summary>
    ///     Lets users sort by the column's property.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A column that computes its value fails at startup with this method; sort it with
    ///     <see cref="Sortable{TSort}(Expression{Func{TResource, TSort}})" /> instead.
    /// </remarks>
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
    ///     Lets users sort the column by a different expression.
    /// </summary>
    /// <typeparam name="TSort">The type of the value to sort by.</typeparam>
    /// <param name="selector">An expression that selects the value to sort by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="selector" /> is <see langword="null" />.
    /// </exception>
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
    ///     Renders the column's cells with a display and configures it.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <param name="configure">A delegate that configures the display.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///     <typeparamref name="TDisplay" /> has a handler.
    /// </exception>
    /// <remarks>
    ///     Without a display, the cells render the grid display template their property's
    ///     <c>[UIHint]</c> names, or the default display for its type. Columns don't support
    ///     displays with a handler.
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
    ///     Renders the column's cells with a display.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="NotSupportedException">
    ///     <typeparamref name="TDisplay" /> has a handler.
    /// </exception>
    public ResourceColumnBuilder<TResource> UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    /// <summary>
    ///     Renders the cells from the property's metadata instead of a display.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
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
