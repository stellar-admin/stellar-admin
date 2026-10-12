using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Dashboard.Resources.Displays;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a read-only form field.
/// </summary>
public sealed class ResourceDisplayFieldBuilder
{
    private readonly List<Action<FormDisplayFieldOptions>> _configuration = [];
    private readonly LambdaExpression _field;
    private readonly string _fieldName;
    private readonly PropertyInfo[] _propertyPath;

    /// <summary>
    ///     The text shown when the value is empty. Defaults to the property's
    ///     <c>[DisplayFormat(NullDisplayText)]</c>, then a dash.
    /// </summary>
    public string? EmptyText
    {
        set => _configuration.Add(options => options.EmptyText = value);
    }

    /// <summary>
    ///     The composite format string for the value, such as <c>{0:C}</c>. Defaults to the
    ///     property's <c>[DisplayFormat(DataFormatString)]</c>.
    /// </summary>
    public string? Format
    {
        set => _configuration.Add(options => options.Format = value);
    }

    /// <summary>
    ///     The field label.
    /// </summary>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    private ResourceDisplayFieldBuilder(
        LambdaExpression field,
        string fieldName,
        PropertyInfo[] propertyPath
    )
    {
        _field = field;
        _fieldName = fieldName;
        _propertyPath = propertyPath;
    }

    /// <summary>
    ///     Sets the number of columns the field spans.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
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
    public ResourceDisplayFieldBuilder ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        var columnSpan = ColumnSpanBuilder.Build(configure);
        _configuration.Add(options => options.ColumnSpan = columnSpan);

        return this;
    }

    /// <summary>
    ///     Spans all columns.
    /// </summary>
    public ResourceDisplayFieldBuilder ColumnSpanFull()
    {
        _configuration.Add(options => options.ColumnSpan = new(Base: ResponsiveGrid.FullSpan));

        return this;
    }

    /// <summary>
    ///     Sets the column the field starts at.
    /// </summary>
    /// <remarks>
    ///     Defaults to following the item before it. Ignored where the grid has one column or the
    ///     field would extend past the last column.
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
    public ResourceDisplayFieldBuilder ColumnStart(Action<ColumnStartBuilder> configure)
    {
        var columnStart = ColumnStartBuilder.Build(configure);
        _configuration.Add(options => options.ColumnStart = columnStart);

        return this;
    }

    /// <summary>
    ///     Uses and configures a display for this field.
    /// </summary>
    /// <remarks>
    ///     Without a display, the value renders from the property's metadata, or the display
    ///     template its <c>[UIHint]</c> names.
    /// </remarks>
    public ResourceDisplayFieldBuilder UseDisplay<TDisplay>(Action<TDisplay> configure)
        where TDisplay : FieldDisplay, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configuration.Add(options =>
        {
            var display = new TDisplay();
            configure(display);
            options.Display = display;
        });

        return this;
    }

    /// <summary>
    ///     Uses a display for this field.
    /// </summary>
    public ResourceDisplayFieldBuilder UseDisplay<TDisplay>()
        where TDisplay : FieldDisplay, new() => UseDisplay<TDisplay>(_ => { });

    // Display fields are never bound, so unlike editable fields they need no setter
    internal static ResourceDisplayFieldBuilder Create(LambdaExpression field, string paramName)
    {
        ArgumentNullException.ThrowIfNull(field, paramName);

        var properties = ResourcePropertyPath.GetProperties(field);
        if (properties is null || properties.Any(property => property.GetMethod?.IsPublic != true))
        {
            throw new ArgumentException("Select a property path with public getters.", paramName);
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
