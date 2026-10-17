using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Resources.Displays;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a read-only form field.
/// </summary>
public sealed class FormDisplayFieldOptions : FormItemOptions
{
    /// <summary>
    ///     Gets the display that renders the field's value.
    /// </summary>
    /// <value>
    ///     The display, or <see langword="null" /> to use the template the property's
    ///     <c>[UIHint]</c> names or the default display for its type.
    /// </value>
    public FieldDisplay? Display { get; internal set; }

    /// <summary>
    ///     Gets the text shown when the value is empty.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use the property's
    ///     <c>[DisplayFormat(NullDisplayText)]</c>.
    /// </value>
    public string? EmptyText { get; internal set; }

    /// <summary>
    ///     Gets the expression that selects the property the field displays.
    /// </summary>
    public LambdaExpression FieldExpression { get; }

    /// <summary>
    ///     Gets the property path that <see cref="FieldExpression" /> selects.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    ///     Gets whether the label displays above or beside the value.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the orientation, or
    ///     <see langword="null" /> to use the orientation of the field's section.
    /// </value>
    public DisplayFieldOrientation? Orientation { get; internal set; }

    /// <summary>
    ///     Gets the properties that <see cref="FieldExpression" /> selects, in order.
    /// </summary>
    public IReadOnlyList<PropertyInfo> PropertyPath { get; }

    /// <summary>
    ///     Gets the field's label.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the property's display name.
    /// </value>
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
