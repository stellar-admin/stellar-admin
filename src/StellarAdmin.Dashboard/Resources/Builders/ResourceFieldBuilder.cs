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
    public ResourceFieldBuilder UseEditor<TEditor>(Action<TEditor> configure)
        where TEditor : ResourceEditor, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configuration.Add(options =>
        {
            var editor = new TEditor();
            configure(editor);
            options.Editor = editor;
        });

        return this;
    }

    /// <summary>
    ///     Uses an editor for this field.
    /// </summary>
    public ResourceFieldBuilder UseEditor<TEditor>()
        where TEditor : ResourceEditor, new() => UseEditor<TEditor>(_ => { });

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
