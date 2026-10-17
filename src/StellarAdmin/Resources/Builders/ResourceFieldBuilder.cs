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
    ///     Sets the help text shown with the field.
    /// </summary>
    /// <value>
    ///     The default is the property's <c>[Display(Description)]</c>.
    /// </value>
    public string? Description
    {
        set => _configuration.Add(options => options.Description = value);
    }

    /// <summary>
    ///     Sets the field label.
    /// </summary>
    /// <value>
    ///     The default is the property's display name.
    /// </value>
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
    ///     Sets the number of columns the field spans at every width.
    /// </summary>
    /// <param name="span">The number of columns, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="span" /> is less than 1 or greater than 12.
    /// </exception>
    /// <remarks>
    ///     The default is 1.
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
    /// <param name="configure">A delegate that sets the span at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        var columnSpan = ColumnSpanBuilder.Build(configure);
        _configuration.Add(options => options.ColumnSpan = columnSpan);

        return this;
    }

    /// <summary>
    ///     Makes the field span all columns.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder ColumnSpanFull()
    {
        _configuration.Add(options => options.ColumnSpan = new(Base: ResponsiveGrid.FullSpan));

        return this;
    }

    /// <summary>
    ///     Sets the column the field starts at, at every width.
    /// </summary>
    /// <param name="start">The column number, from 1 to 12.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="start" /> is less than 1 or greater than 12.
    /// </exception>
    /// <remarks>
    ///     By default, the field follows the item before it. The start is ignored where the grid
    ///     has one column or the field would extend past the last column.
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
    /// <param name="configure">A delegate that sets the start at each breakpoint.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder ColumnStart(Action<ColumnStartBuilder> configure)
    {
        var columnStart = ColumnStartBuilder.Build(configure);
        _configuration.Add(options => options.ColumnStart = columnStart);

        return this;
    }

    /// <summary>
    ///     Edits the field with an editor and configures it.
    /// </summary>
    /// <typeparam name="TEditor">The type of the editor.</typeparam>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder UseEditor<TEditor>(Action<TEditor> configure)
        where TEditor : FieldEditor, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configuration.Add(options =>
        {
            var editor = new TEditor();
            configure(editor);
            var handlerType = (editor as IFieldEditor)?.HandlerType;
            if (
                handlerType is not null
                && !typeof(IFieldEditorHandler<TEditor>).IsAssignableFrom(handlerType)
            )
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
    ///     Edits the field with an editor.
    /// </summary>
    /// <typeparam name="TEditor">The type of the editor.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder UseEditor<TEditor>()
        where TEditor : FieldEditor, new() => UseEditor<TEditor>(_ => { });

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
