using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a form field.
/// </summary>
public sealed class FormFieldOptions : FormItemOptions
{
    /// <summary>
    ///     Gets the help text shown with the field.
    /// </summary>
    /// <value>
    ///     The help text, or <see langword="null" /> to use the property's description.
    /// </value>
    public string? Description { get; internal set; }

    /// <summary>
    ///     Gets the editor that renders the field.
    /// </summary>
    /// <value>
    ///     The editor, or <see langword="null" /> to render the editor template for the property's
    ///     type.
    /// </value>
    public FieldEditor? Editor { get; internal set; }

    /// <summary>
    ///     Gets the expression that selects the property the field edits.
    /// </summary>
    public LambdaExpression FieldExpression { get; }

    /// <summary>
    ///     Gets the property path that <see cref="FieldExpression" /> selects.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    ///     Gets a value that indicates whether the field is read-only.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the field is read-only and a posted value for it is ignored;
    ///     otherwise, <see langword="false" />.
    /// </value>
    public bool IsReadOnly { get; internal set; }

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

    internal FormFieldOptions(
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
