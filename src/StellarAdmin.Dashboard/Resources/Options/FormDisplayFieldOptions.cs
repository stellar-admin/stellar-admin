using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Dashboard.Resources.Displays;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A configured read-only field of a form page.
/// </summary>
public sealed class FormDisplayFieldOptions : FormItemOptions
{
    /// <summary>
    ///     The display's configuration, or <c>null</c> to render the value from the property's
    ///     metadata.
    /// </summary>
    public FieldDisplay? Display { get; internal set; }

    /// <summary>
    ///     The text shown when the value is empty, or <c>null</c> to derive it from the property's
    ///     metadata.
    /// </summary>
    public string? EmptyText { get; internal set; }

    /// <summary>
    ///     The expression selecting the property the field displays.
    /// </summary>
    public LambdaExpression FieldExpression { get; }

    /// <summary>
    ///     The property path selected by <see cref="FieldExpression" />.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    ///     The composite format string for the value, or <c>null</c> to derive it from the
    ///     property's metadata.
    /// </summary>
    public string? Format { get; internal set; }

    /// <summary>
    ///     The properties selected by <see cref="FieldExpression" />, in order.
    /// </summary>
    public IReadOnlyList<PropertyInfo> PropertyPath { get; }

    /// <summary>
    ///     The field label, or <c>null</c> to derive it from the property's metadata.
    /// </summary>
    public string? Title { get; internal set; }

    internal FormDisplayFieldOptions(
        LambdaExpression fieldExpression,
        string fieldName,
        PropertyInfo[] propertyPath
    )
    {
        FieldExpression = fieldExpression;
        FieldName = fieldName;
        PropertyPath = Array.AsReadOnly((PropertyInfo[])propertyPath.Clone());
    }
}
