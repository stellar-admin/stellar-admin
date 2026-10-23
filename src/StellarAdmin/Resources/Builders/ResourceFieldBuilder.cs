using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a form field.
/// </summary>
/// <typeparam name="TResource">The type of the resource the form edits.</typeparam>
public sealed class ResourceFieldBuilder<TResource>
{
    private readonly List<Action<FormFieldOptions>> _configuration = [];
    private readonly LambdaExpression _field;
    private readonly string _fieldName;
    private readonly PropertyInfo[] _propertyPath;
    private readonly ConfigurationSource? _source = ConfigurationSource.Capture();

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
    /// <remarks>
    ///     The default is 1.
    /// </remarks>
    public ResourceFieldBuilder<TResource> ColumnSpan(int span)
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
    public ResourceFieldBuilder<TResource> ColumnSpan(Action<ColumnSpanBuilder> configure)
    {
        var columnSpan = ColumnSpanBuilder.Build(configure);
        _configuration.Add(options => options.ColumnSpan = columnSpan);

        return this;
    }

    /// <summary>
    ///     Makes the field span all columns.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> ColumnSpanFull()
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
    public ResourceFieldBuilder<TResource> ColumnStart(int start)
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
    public ResourceFieldBuilder<TResource> ColumnStart(Action<ColumnStartBuilder> configure)
    {
        var columnStart = ColumnStartBuilder.Build(configure);
        _configuration.Add(options => options.ColumnStart = columnStart);

        return this;
    }

    /// <summary>
    ///     Makes the field's items depend on another field's value.
    /// </summary>
    /// <typeparam name="TProperty">The type of the other field's property.</typeparam>
    /// <param name="field">An expression that selects the other field's property.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         When the other field changes, the field loads its items again and its value is
    ///         cleared. While the other field has no value, the field is disabled. Call this once
    ///         for each field the items depend on.
    ///     </para>
    ///     <para>
    ///         The form the items load with holds only the values of the fields declared here,
    ///         and the key on the edit form; any other property has its default value. The other
    ///         field must be an editable field of the same form, or the dependency is a
    ///         configuration problem and is ignored.
    ///     </para>
    /// </remarks>
    public ResourceFieldBuilder<TResource> DependsOn<TProperty>(
        Expression<Func<TResource, TProperty>> field
    ) => DependsOn(field, _ => { });

    /// <summary>
    ///     Makes the field's items depend on another field's value, and configures the dependency.
    /// </summary>
    /// <typeparam name="TProperty">The type of the other field's property.</typeparam>
    /// <param name="field">An expression that selects the other field's property.</param>
    /// <param name="configure">A delegate that configures the dependency.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     See <see cref="DependsOn{TProperty}(Expression{Func{TResource, TProperty}})" />.
    /// </remarks>
    public ResourceFieldBuilder<TResource> DependsOn<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<FieldDependencyBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(configure);

        var source = ConfigurationSource.Capture();
        var properties = ResourcePropertyPath.GetProperties(field);
        _configuration.Add(options =>
        {
            if (properties is null)
            {
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.InvalidSelector,
                    $"The `{options.FieldName}` field doesn't depend on `{field}` because it doesn't select a property path.",
                    "Select a property of the model, or a path of properties.",
                    source
                );

                return;
            }

            var dependency = new FieldDependency(ResourcePropertyPath.GetName(properties))
            {
                Source = source,
            };
            configure(new FieldDependencyBuilder(dependency));
            options.Dependencies.Add(dependency);
        });

        return this;
    }

    /// <summary>
    ///     Edits the field with a checkbox.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseCheckboxEditor() => UseCheckboxEditor(_ => { });

    /// <summary>
    ///     Edits the field with a checkbox and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseCheckboxEditor(Action<CheckboxEditor> configure) =>
        Use(() => new CheckboxEditor(), configure);

    /// <summary>
    ///     Edits the field with a group of checkboxes.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseCheckboxGroupEditor() => UseCheckboxGroupEditor(_ => { });

    /// <summary>
    ///     Edits the field with a group of checkboxes and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseCheckboxGroupEditor(Action<CheckboxGroupEditor<TResource>> configure) =>
        Use(() => new CheckboxGroupEditor<TResource>(), configure);

    /// <summary>
    ///     Edits the field with a date input.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseDateInputEditor() => UseDateInputEditor(_ => { });

    /// <summary>
    ///     Edits the field with a date input and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseDateInputEditor(Action<DateInputEditor> configure) =>
        Use(() => new DateInputEditor(), configure);

    /// <summary>
    ///     Edits the field with a date and time input.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseDateTimeInputEditor() => UseDateTimeInputEditor(_ => { });

    /// <summary>
    ///     Edits the field with a date and time input and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseDateTimeInputEditor(Action<DateTimeInputEditor> configure) =>
        Use(() => new DateTimeInputEditor(), configure);

    /// <summary>
    ///     Edits the field with an editor and configures it.
    /// </summary>
    /// <typeparam name="TEditor">The type of the editor.</typeparam>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Use this for custom editors. The built-in editors have their own methods, such as
    ///     <see cref="UseSelectEditor(Action{SelectEditor{TResource}})" />.
    /// </remarks>
    public ResourceFieldBuilder<TResource> UseEditor<TEditor>(Action<TEditor> configure)
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
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.MismatchedHandler,
                    $"The `{options.FieldName}` field doesn't use `{typeof(TEditor).Name}` because its handler `{handlerType.Name}` doesn't implement `IFieldEditorHandler<{typeof(TEditor).Name}>`. It keeps its default editor.",
                    $"Implement `IFieldEditorHandler<{typeof(TEditor).Name}>` on `{handlerType.Name}`.",
                    options.Source
                );

                return;
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
    public ResourceFieldBuilder<TResource> UseEditor<TEditor>()
        where TEditor : FieldEditor, new() => UseEditor<TEditor>(_ => { });

    /// <summary>
    ///     Edits the field with an icon editor.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseIconEditor() => UseIconEditor(_ => { });

    /// <summary>
    ///     Edits the field with an icon editor and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseIconEditor(Action<IconEditor> configure) =>
        Use(() => new IconEditor(), configure);

    /// <summary>
    ///     Edits the field with an image editor.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseImageEditor() => UseImageEditor(_ => { });

    /// <summary>
    ///     Edits the field with an image editor and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseImageEditor(Action<ImageEditor> configure) =>
        Use(() => new ImageEditor(), configure);

    /// <summary>
    ///     Edits the field with a lookup sheet.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseLookupSheetEditor() => UseLookupSheetEditor(_ => { });

    /// <summary>
    ///     Edits the field with a lookup sheet and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseLookupSheetEditor(Action<LookupSheetEditor<TResource>> configure) =>
        Use(() => new LookupSheetEditor<TResource>(), configure);

    /// <summary>
    ///     Edits the field with a multi-select lookup sheet.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseMultiLookupSheetEditor() => UseMultiLookupSheetEditor(_ => { });

    /// <summary>
    ///     Edits the field with a multi-select lookup sheet and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseMultiLookupSheetEditor(Action<MultiLookupSheetEditor<TResource>> configure) =>
        Use(() => new MultiLookupSheetEditor<TResource>(), configure);

    /// <summary>
    ///     Edits the field with a one-time code input.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseOneTimeCodeEditor() => UseOneTimeCodeEditor(_ => { });

    /// <summary>
    ///     Edits the field with a one-time code input and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseOneTimeCodeEditor(Action<OneTimeCodeEditor> configure) =>
        Use(() => new OneTimeCodeEditor(), configure);

    /// <summary>
    ///     Edits the field with a radio group.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseRadioGroupEditor() => UseRadioGroupEditor(_ => { });

    /// <summary>
    ///     Edits the field with a radio group and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseRadioGroupEditor(Action<RadioGroupEditor<TResource>> configure) =>
        Use(() => new RadioGroupEditor<TResource>(), configure);

    /// <summary>
    ///     Edits the field with a rich text editor.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseRichTextEditor() => UseRichTextEditor(_ => { });

    /// <summary>
    ///     Edits the field with a rich text editor and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseRichTextEditor(Action<RichTextEditor> configure) =>
        Use(() => new RichTextEditor(), configure);

    /// <summary>
    ///     Edits the field with a segmented control.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseSegmentedControlEditor() => UseSegmentedControlEditor(_ => { });

    /// <summary>
    ///     Edits the field with a segmented control and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseSegmentedControlEditor(Action<SegmentedControlEditor<TResource>> configure) =>
        Use(() => new SegmentedControlEditor<TResource>(), configure);

    /// <summary>
    ///     Edits the field with a select.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseSelectEditor() => UseSelectEditor(_ => { });

    /// <summary>
    ///     Edits the field with a select and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseSelectEditor(Action<SelectEditor<TResource>> configure) =>
        Use(() => new SelectEditor<TResource>(), configure);

    /// <summary>
    ///     Edits the field with a slider.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseSliderEditor() => UseSliderEditor(_ => { });

    /// <summary>
    ///     Edits the field with a slider and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseSliderEditor(Action<SliderEditor> configure) =>
        Use(() => new SliderEditor(), configure);

    /// <summary>
    ///     Edits the field with a text area.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseTextareaEditor() => UseTextareaEditor(_ => { });

    /// <summary>
    ///     Edits the field with a text area and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseTextareaEditor(Action<TextareaEditor> configure) =>
        Use(() => new TextareaEditor(), configure);

    /// <summary>
    ///     Edits the field with a text input.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseTextInputEditor() => UseTextInputEditor(_ => { });

    /// <summary>
    ///     Edits the field with a text input and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseTextInputEditor(Action<TextInputEditor> configure) =>
        Use(() => new TextInputEditor(), configure);

    /// <summary>
    ///     Edits the field with a time input.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseTimeInputEditor() => UseTimeInputEditor(_ => { });

    /// <summary>
    ///     Edits the field with a time input and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseTimeInputEditor(Action<TimeInputEditor> configure) =>
        Use(() => new TimeInputEditor(), configure);

    /// <summary>
    ///     Edits the field with an on and off switch.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseToggleEditor() => UseToggleEditor(_ => { });

    /// <summary>
    ///     Edits the field with an on and off switch and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseToggleEditor(Action<ToggleEditor> configure) =>
        Use(() => new ToggleEditor(), configure);

    /// <summary>
    ///     Edits the field with a toggle group.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public ResourceFieldBuilder<TResource> UseToggleGroupEditor() => UseToggleGroupEditor(_ => { });

    /// <summary>
    ///     Edits the field with a toggle group and configures it.
    /// </summary>
    /// <param name="configure">A delegate that configures the editor.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceFieldBuilder<TResource> UseToggleGroupEditor(Action<ToggleGroupEditor<TResource>> configure) =>
        Use(() => new ToggleGroupEditor<TResource>(), configure);

    internal FormFieldOptions Build()
    {
        var options = new FormFieldOptions(_field, _fieldName, _propertyPath) { Source = _source };
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }

    private ResourceFieldBuilder<TResource> Use<TEditor>(
        Func<TEditor> create,
        Action<TEditor> configure
    )
        where TEditor : FieldEditor
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configuration.Add(options =>
        {
            var editor = create();
            configure(editor);
            options.Editor = editor;
            options.HandlerType = (editor as IFieldEditor)?.HandlerType;
        });

        return this;
    }
}
