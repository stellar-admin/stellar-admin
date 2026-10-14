using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Resources.Displays;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     A configured read-only field of a form page.
/// </summary>
public sealed class FormDisplayFieldOptions : FormItemOptions
{
    /// <summary>
    ///     The display's configuration, or <c>null</c> to use the property's <c>[UIHint]</c>
    ///     template or the default display for its type.
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
    ///     Whether the label displays above or beside the value, or <c>null</c> to use the
    ///     orientation of the field's section.
    /// </summary>
    public FieldOrientation? Orientation { get; internal set; }

    /// <summary>
    ///     The properties selected by <see cref="FieldExpression" />, in order.
    /// </summary>
    public IReadOnlyList<PropertyInfo> PropertyPath { get; }

    /// <summary>
    ///     The field label, or <c>null</c> to derive it from the property's metadata.
    /// </summary>
    public string? Title { get; internal set; }

    internal Type? HandlerType { get; set; }

    internal bool UsesBuiltInRendering { get; set; }

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
