using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a read-only form field.
/// </summary>
public sealed class ResourceDisplayFieldBuilder
{
    private readonly List<Action<FormDisplayFieldOptions>> _configuration = [];
    private readonly LambdaExpression _field;
    private readonly string? _fieldName;
    private readonly PropertyInfo[] _propertyPath;

    /// <summary>
    ///     Sets the text shown when the value is empty.
    /// </summary>
    /// <value>
    ///     The default is the property's <c>[DisplayFormat(NullDisplayText)]</c>, or a dash when
    ///     the property doesn't set one.
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
    ///     Sets whether the label displays above or beside the value.
    /// </summary>
    /// <value>
    ///     The default is the orientation of the field's section, or
    ///     <see cref="DisplayFieldOrientation.Vertical" /> when the section doesn't set one.
    /// </value>
    public DisplayFieldOrientation? Orientation
    {
        set => _configuration.Add(options => options.Orientation = value);
    }

    /// <summary>
    ///     Sets the field label.
    /// </summary>
    /// <value>
    ///     The default is the property's display name.
    /// </value>
    /// <remarks>
    ///     A field that computes its value, such as <c>tour =&gt; tour.Nights * tour.NightlyRate</c>,
    ///     has no property to name it, so it requires a title.
    /// </remarks>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    private ResourceDisplayFieldBuilder(
        LambdaExpression field,
        string? fieldName,
        PropertyInfo[] propertyPath
    )
    {
        _field = field;
        _fieldName = fieldName;
        _propertyPath = propertyPath;
    }

    /// <summary>
    ///     Sets the number of columns the field spans at every width.
    /// </summary>
    /// <param name="span">The number of columns, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The default is 1.
    /// </remarks>
    public ResourceDisplayFieldBuilder ColumnSpan(int span)
    {
        var columnSpan = ColumnSpanBuilder.FromSpan(span);
        _configuration.Add(options => options.ColumnSpan = columnSpan);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the field spans at each breakpoint.
    /// </summary>
    /// <param name="configure">A delegate that sets the span at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceDisplayFieldBuilder ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        var columnSpan = ColumnSpanBuilder.Build(configure);
        _configuration.Add(options => options.ColumnSpan = columnSpan);

        return this;
    }

    /// <summary>
    ///     Makes the field span all columns.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceDisplayFieldBuilder ColumnSpanFull()
    {
        _configuration.Add(options => options.ColumnSpan = new(Base: ResponsiveGrid.FullSpan));

        return this;
    }

    /// <summary>
    ///     Sets the column the field starts at, at every width.
    /// </summary>
    /// <param name="start">The column number, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     By default, the field follows the item before it. The start is ignored where the grid
    ///     has one column or the field would extend past the last column.
    /// </remarks>
    public ResourceDisplayFieldBuilder ColumnStart(int start)
    {
        var columnStart = ColumnStartBuilder.FromStart(start);
        _configuration.Add(options => options.ColumnStart = columnStart);

        return this;
    }

    /// <summary>
    ///     Sets the column the field starts at, at each breakpoint.
    /// </summary>
    /// <param name="configure">A delegate that sets the start at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceDisplayFieldBuilder ColumnStart(Action<ColumnStartBuilder> configure)
    {
        var columnStart = ColumnStartBuilder.Build(configure);
        _configuration.Add(options => options.ColumnStart = columnStart);

        return this;
    }

    /// <summary>
    ///     Renders the field with a display and configures it.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <param name="configure">A delegate that configures the display.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Without a display, the field renders the display template its property's
    ///     <c>[UIHint]</c> names, or the default display for its type.
    /// </remarks>
    public ResourceDisplayFieldBuilder UseDisplay<TDisplay>(Action<TDisplay> configure)
        where TDisplay : FieldDisplay, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configuration.Add(options =>
        {
            var display = new TDisplay();
            configure(display);
            var handlerType = (display as IFieldDisplay)?.HandlerType;
            if (
                handlerType is not null
                && !typeof(IFieldDisplayHandler<TDisplay>).IsAssignableFrom(handlerType)
            )
            {
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.MismatchedHandler,
                    $"The field '{options.FieldName ?? options.Title}' uses {typeof(TDisplay).Name} with the handler {handlerType.Name}, which must implement IFieldDisplayHandler<{typeof(TDisplay).Name}>.",
                    "The field keeps its default display."
                );

                return;
            }

            options.Display = display;
            options.HandlerType = handlerType;
            options.UsesBuiltInRendering = false;
        });

        return this;
    }

    /// <summary>
    ///     Renders the field with a display.
    /// </summary>
    /// <typeparam name="TDisplay">The type of the display.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceDisplayFieldBuilder UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    /// <summary>
    ///     Renders the value from the property's metadata instead of a display.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The value is formatted with the property's <c>[DisplayFormat]</c>, a boolean shows an
    ///     icon, and email addresses and web addresses become links.
    /// </remarks>
    public ResourceDisplayFieldBuilder UseBuiltInRendering()
    {
        _configuration.Add(options =>
        {
            options.Display = null;
            options.HandlerType = null;
            options.UsesBuiltInRendering = true;
        });

        return this;
    }

    // Display fields are never bound, so unlike editable fields they need no setter. Any other
    // expression computes the field's value
    internal static ResourceDisplayFieldBuilder Create(LambdaExpression field, string paramName)
    {
        ArgumentNullException.ThrowIfNull(field, paramName);

        var properties = ResourcePropertyPath.GetProperties(field);
        if (properties is null)
        {
            return new(field, null, []);
        }

        // Without public getters the value can't be read by property, so the field computes it from the expression
        if (properties.Any(property => property.GetMethod?.IsPublic != true))
        {
            var computed = new ResourceDisplayFieldBuilder(field, null, []);
            computed._configuration.Add(_ =>
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.InvalidSelector,
                    $"The field '{field}' selects a property without a public getter.",
                    "The field computes its value from the expression, and needs a Title."
                )
            );

            return computed;
        }

        return new(field, ResourcePropertyPath.GetName(properties), properties);
    }

    internal FormDisplayFieldOptions Build()
    {
        var options = new FormDisplayFieldOptions(_field, _fieldName, _propertyPath);
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
