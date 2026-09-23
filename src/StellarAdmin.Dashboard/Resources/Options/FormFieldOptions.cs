using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A configured field of a form page.
/// </summary>
public sealed class FormFieldOptions : FormItemOptions
{
    /// <summary>
    ///     The editor's configuration.
    /// </summary>
    public EditorOptions Editor { get; internal set; } = new();

    /// <summary>
    ///     The expression selecting the property the field renders and binds.
    /// </summary>
    public LambdaExpression FieldExpression { get; }

    /// <summary>
    ///     The property path selected by <see cref="FieldExpression" />.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    ///     Whether the field renders read-only and is excluded from the form's binding
    ///     allow-list — a posted value for it is ignored.
    /// </summary>
    public bool IsReadOnly { get; internal set; }

    /// <summary>
    ///     The field label, or <c>null</c> to derive it from the property's metadata.
    /// </summary>
    public string? Title { get; internal set; }

    internal FormFieldOptions(LambdaExpression fieldExpression, string fieldName)
    {
        FieldExpression = fieldExpression;
        FieldName = fieldName;
    }
}
