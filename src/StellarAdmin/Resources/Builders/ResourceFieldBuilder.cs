using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a form field.
/// </summary>
public sealed class ResourceFieldBuilder
{
    private readonly List<Action<FormFieldOptions>> _configuration = [];
    private readonly LambdaExpression _field;
    private readonly string _fieldName;
    private readonly PropertyInfo[] _propertyPath;

    /// <summary>
    ///     The help text shown with the field. Defaults to the property's
    ///     <c>[Display(Description)]</c>.
    /// </summary>
    public string? Description
    {
        set => _configuration.Add(options => options.Description = value);
    }

    /// <summary>
    ///     The field label.
    /// </summary>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    internal ResourceFieldBuilder(
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
    public ResourceFieldBuilder ColumnSpan(int span)
    {
        var columnSpan = ColumnSpanBuilder.FromSpan(span);
        _configuration.Add(options => options.ColumnSpan = columnSpan);

        return this;
    }

    /// <summary>
    ///     Sets the number of columns the field spans at each breakpoint.
    /// </summary>
    public ResourceFieldBuilder ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        var columnSpan = ColumnSpanBuilder.Build(configure);
        _configuration.Add(options => options.ColumnSpan = columnSpan);

        return this;
    }

    /// <summary>
    ///     Spans all columns.
    /// </summary>
    public ResourceFieldBuilder ColumnSpanFull()
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
    public ResourceFieldBuilder ColumnStart(int start)
    {
        var columnStart = ColumnStartBuilder.FromStart(start);
        _configuration.Add(options => options.ColumnStart = columnStart);

        return this;
    }

    /// <summary>
    ///     Sets the column the field starts at, at each breakpoint.
    /// </summary>
    public ResourceFieldBuilder ColumnStart(Action<ColumnStartBuilder> configure)
    {
        var columnStart = ColumnStartBuilder.Build(configure);
        _configuration.Add(options => options.ColumnStart = columnStart);

        return this;
    }

    /// <summary>
    ///     Uses and configures an editor for this field.
    /// </summary>
    public ResourceFieldBuilder UseEditor<TEditor>(Action<TEditor> configure)
        where TEditor : FieldEditor, IFieldEditor, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configuration.Add(options =>
        {
            var editor = new TEditor();
            configure(editor);
            var handlerType = editor.HandlerType;
            if (!typeof(IFieldEditorHandler<TEditor>).IsAssignableFrom(handlerType))
            {
                throw new InvalidOperationException(
                    $"{handlerType.Name} must implement IFieldEditorHandler<{typeof(TEditor).Name}>."
                );
            }

            options.Editor = editor;
            options.HandlerType = handlerType;
        });

        return this;
    }

    /// <summary>
    ///     Uses an editor for this field.
    /// </summary>
    public ResourceFieldBuilder UseEditor<TEditor>()
        where TEditor : FieldEditor, IFieldEditor, new() => UseEditor<TEditor>(_ => { });

    internal FormFieldOptions Build()
    {
        var options = new FormFieldOptions(_field, _fieldName, _propertyPath);
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
