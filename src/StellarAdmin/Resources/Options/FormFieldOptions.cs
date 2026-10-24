using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Primitives;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a form field.
/// </summary>
public sealed class FormFieldOptions : FormItemOptions
{
    /// <summary>
    ///     Gets the binder that reads and binds the field.
    /// </summary>
    /// <value>
    ///     The binder, or <see langword="null" /> to bind the field's property.
    /// </value>
    /// <remarks>
    ///     <see cref="UseBinder" /> sets the binder.
    /// </remarks>
    public FieldBinder? Binder { get; private set; }

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
    ///     Gets a value that indicates whether the field is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the field is disabled, so its editor doesn't post and a posted
    ///     value for it is ignored; otherwise, <see langword="false" />.
    /// </value>
    public bool IsDisabled { get; internal set; }

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

    // The fields whose values the field's items depend on, in the order they were declared
    internal List<FieldDependency> Dependencies { get; } = [];

    internal Type? HandlerType { get; set; }

    internal bool IsAlwaysReadOnly { get; set; }

    // A disabled field, or one read-only for every record, so the configuration checks that need an editable field skip
    // it. A field a read-only predicate decides is editable for them.
    internal bool IsNeverEditable => IsAlwaysReadOnly || IsDisabled;

    // Decides for each record whether the field is read-only, or null when only IsAlwaysReadOnly does
    internal Func<object, bool>? ReadOnlyWhen { get; set; }

    // The resource an integration inferred for the field while it had no editor. The field gets a sheet editor for it
    // once the registered resources are known.
    internal ReferencedResource? InferredResource { get; set; }

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

    /// <summary>
    ///     Takes the items of the field's editor from a resource, when the editor is a choice or
    ///     lookup editor without items, or the field has no editor.
    /// </summary>
    /// <param name="resourceType">The type of the resource.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resourceType" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Integrations call it from a post-configuration callback for a field whose related records
    ///     they know, such as from a foreign key. An editor with items keeps them. A field with no
    ///     editor gets a lookup sheet, or a multi-lookup sheet for a collection, when the resource is
    ///     registered.
    /// </remarks>
    public void InferResource(Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var resource = ReferencedResource.Create(resourceType, Source);
        switch (Editor)
        {
            case ChoiceEditor { ItemsLoader: null } choice:
                choice.UseResource(resource);
                break;
            case LookupSheetEditor { Items: null, Resource: null } lookup:
                lookup.UseResource(resource);
                break;
            case MultiLookupSheetEditor { Items: null, Resource: null } lookup:
                lookup.UseResource(resource);
                break;
            case null:
                InferredResource = resource;
                break;
        }
    }

    /// <summary>
    ///     Determines whether the field is configured as read-only for a record.
    /// </summary>
    /// <param name="model">The model the form edits.</param>
    /// <returns>
    ///     <see langword="true" /> if the field is read-only, so a posted value for it is ignored;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="model" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The property's metadata, such as <c>[ReadOnly(true)]</c>, also makes a field without a
    ///     binder read-only.
    /// </remarks>
    public bool IsReadOnly(object model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return IsAlwaysReadOnly || ReadOnlyWhen?.Invoke(model) == true;
    }

    /// <summary>
    ///     Sets the binder that reads and binds the field in place of its property.
    /// </summary>
    /// <param name="binder">The binder.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="binder" /> is <see langword="null" />.
    /// </exception>
    public void UseBinder(FieldBinder binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        Binder = binder;
    }

    // The value the field's editor shows: the binder's, or else the property's
    internal object? GetValue(object model, IServiceProvider services) =>
        Binder is { } binder
            ? binder.GetValue(new FieldBindingContext(this, model, StringValues.Empty, services))
            : ResourcePropertyPath.GetValue(model, PropertyPath);
}
