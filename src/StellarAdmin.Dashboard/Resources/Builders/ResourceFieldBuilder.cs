using System.Linq.Expressions;
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

    /// <summary>
    ///     The field label.
    /// </summary>
    public string? Title
    {
        set => _configuration.Add(options => options.Title = value);
    }

    internal ResourceFieldBuilder(LambdaExpression field, string fieldName)
    {
        _field = field;
        _fieldName = fieldName;
    }

    /// <summary>
    ///     Uses and configures an editor for this field.
    /// </summary>
    public ResourceFieldBuilder UseEditor<TOptions>(Action<TOptions> configure)
        where TOptions : EditorOptions, IFieldEditorOptions, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configuration.Add(options =>
        {
            var editorOptions = new TOptions();
            configure(editorOptions);
            var editorType = editorOptions.EditorType;
            if (!typeof(IFieldEditor<TOptions>).IsAssignableFrom(editorType))
            {
                throw new InvalidOperationException(
                    $"{editorType.Name} must implement IFieldEditor<{typeof(TOptions).Name}>."
                );
            }

            options.Editor = editorOptions;
            options.EditorType = editorType;
        });

        return this;
    }

    /// <summary>
    ///     Uses an editor for this field.
    /// </summary>
    public ResourceFieldBuilder UseEditor<TOptions>()
        where TOptions : EditorOptions, IFieldEditorOptions, new() => UseEditor<TOptions>(_ => { });

    internal FormFieldOptions Build()
    {
        var options = new FormFieldOptions(_field, _fieldName);
        foreach (var configure in _configuration)
        {
            configure(options);
        }

        return options;
    }
}
