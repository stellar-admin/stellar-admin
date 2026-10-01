using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

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
