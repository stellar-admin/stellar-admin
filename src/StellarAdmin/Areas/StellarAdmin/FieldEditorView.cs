using Microsoft.AspNetCore.Mvc.Razor;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     Provides the base class for editor templates, with access to the field's settings.
/// </summary>
/// <typeparam name="TEditor">The type of the editor that the template renders.</typeparam>
public abstract class FieldEditorView<TEditor> : RazorPage<object?>
    where TEditor : FieldEditor, new()
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
    public bool IsReadOnly => Field?.IsReadOnly == true || ViewData.ModelMetadata.IsReadOnly;

    /// <summary>
    ///     Gets the field's label, or <see langword="null" /> to use the property's display name.
    /// </summary>
    public string? Title => Field?.Title;

    private TEditor ResolveEditor()
    {
        switch (Field?.Editor)
        {
            case TEditor editor:
                return editor;
            case null:
                return new TEditor();
        }

        if (Field.Editor.GetType() != typeof(FieldEditor))
        {
            throw new InvalidOperationException(
                $"{Field.Editor.GetType().Name} is not supported by a template for {typeof(TEditor).Name}."
            );
        }

        // A field without a selected editor can still carry class names
        var defaults = new TEditor();
        Field.Editor.ClassNames.CopyTo(defaults.ClassNames);

        return defaults;
    }
}
