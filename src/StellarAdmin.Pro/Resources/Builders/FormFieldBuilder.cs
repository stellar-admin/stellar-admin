using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures a single field of a form page.
/// </summary>
public class FormFieldBuilder
{
    private readonly FormFieldOptions _field;

    internal FormFieldBuilder(FormFieldOptions field)
    {
        _field = field;
    }

    /// <summary>
    ///     Configures the editor without changing its template.
    /// </summary>
    public FormFieldBuilder Editor(Action<EditorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(_field.Editor);

        return this;
    }

    /// <summary>
    ///     Configures editor-specific options without changing its template.
    /// </summary>
    public FormFieldBuilder Editor<TOptions>(Action<TOptions> configure)
        where TOptions : EditorOptions, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (_field.Editor is TOptions existing)
        {
            configure(existing);

            return this;
        }

        if (_field.Editor.GetType() != typeof(EditorOptions))
        {
            throw new InvalidOperationException(
                $"Field '{_field.FieldName}' already uses {_field.Editor.GetType().Name}; it cannot also use {typeof(TOptions).Name}."
            );
        }

        var options = new TOptions();
        _field.Editor.ClassNames.CopyTo(options.ClassNames);
        configure(options);
        _field.Editor = options;

        return this;
    }

    /// <summary>
    ///     Makes the field read-only. It renders without an editable input, and a posted
    ///     value for it is ignored.
    /// </summary>
    public FormFieldBuilder ReadOnly()
    {
        _field.IsReadOnly = true;

        return this;
    }

    /// <summary>
    ///     Sets the editor template that renders the field, overriding a <c>[UIHint]</c> on
    ///     the property.
    /// </summary>
    public FormFieldBuilder Template(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        _field.Template = template;

        return this;
    }

    /// <summary>
    ///     Sets the field label, overriding the label derived from the property metadata.
    /// </summary>
    public FormFieldBuilder Title(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        _field.Title = title;

        return this;
    }
}
