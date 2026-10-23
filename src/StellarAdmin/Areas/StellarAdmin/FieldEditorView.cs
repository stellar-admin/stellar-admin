using Microsoft.AspNetCore.Mvc.Razor;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     Provides the base class for editor templates, with access to the field's settings.
/// </summary>
/// <typeparam name="TEditor">The type of the editor that the template renders.</typeparam>
public abstract class FieldEditorView<TEditor> : RazorPage<object?>
    where TEditor : FieldEditor
{
    /// <summary>
    ///     Gets the field's help text, or <see langword="null" /> to use the property's
    ///     description.
    /// </summary>
    public string? Description => Field?.Description;

    /// <summary>
    ///     Gets the ID of the error shown under the editor in a grid cell, or
    ///     <see langword="null" /> when the field has no error.
    /// </summary>
    /// <remarks>
    ///     Point the editor's <c>aria-describedby</c> at this ID in a grid template.
    /// </remarks>
    public string? CellErrorId => GridEditorRenderer.GetErrorId(ViewData);

    /// <summary>
    ///     Gets the field's editor settings.
    /// </summary>
    /// <value>
    ///     The field's editor, or an editor with default settings when the field has none.
    /// </value>
    /// <exception cref="InvalidOperationException">
    ///     The field uses an editor of another type.
    /// </exception>
    public TEditor Editor => field ??= ResolveEditor();

    /// <summary>
    ///     Gets the data prepared for the editor on the current request, or
    ///     <see langword="null" /> when there is none.
    /// </summary>
    public object? EditorData => Field?.EditorData;

    /// <summary>
    ///     Gets the field's settings, or <see langword="null" /> when the template renders outside
    ///     a resource form.
    /// </summary>
    public FormFieldProperties? Field =>
        ViewData[ViewDataKeys.FormFieldProperties] as FormFieldProperties;

    /// <summary>
    ///     Gets a value that indicates whether the field is read-only.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the field or its property is read-only; otherwise,
    ///     <see langword="false" />.
    /// </value>
    public bool IsReadOnly =>
        Field?.IsReadOnly == true || Field?.IsBound != true && ViewData.ModelMetadata.IsReadOnly;

    /// <summary>
    ///     Gets the field's label, or <see langword="null" /> to use the property's display name.
    /// </summary>
    public string? Title => Field?.Title;

    // The text the field shows, disabled, while a field its items depend on has no value
    internal string? WaitingText => Field?.WaitingText;

    private TEditor ResolveEditor()
    {
        return Field?.Editor switch
        {
            TEditor editor => editor,
            // The built-in editors that take items have no public constructor
            null => (TEditor)Activator.CreateInstance(typeof(TEditor), nonPublic: true)!,
            var other => throw new ConfigurationException(
                ConfigurationProblemKind.TemplateMismatch,
                $"its editor is a `{other.TypeName}`, but it renders with the template for `{typeof(TEditor).Name}`",
                $"Render the field with the template for `{other.TypeName}`, or give it a `{typeof(TEditor).Name}`."
            ),
        };
    }
}
