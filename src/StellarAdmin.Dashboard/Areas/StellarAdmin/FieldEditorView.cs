using Microsoft.AspNetCore.Mvc.Razor;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

/// <summary>
///     Base class for field editor templates, exposing the field's configured properties.
/// </summary>
/// <typeparam name="TEditor">The editor configuration the template renders.</typeparam>
public abstract class FieldEditorView<TEditor> : RazorPage<object?>
    where TEditor : FieldEditor, new()
{
    /// <summary>
    ///     The field's help text, or null to use the property's description.
    /// </summary>
    public string? Description => Field?.Description;

    /// <summary>
    ///     The field's editor configuration. A field without one gets default settings.
    /// </summary>
    public TEditor Editor => field ??= ResolveEditor();

    /// <summary>
    ///     Data prepared for this editor on the current request.
    /// </summary>
    public object? EditorData => Field?.EditorData;

    /// <summary>
    ///     The configured field properties, or null when the template renders outside a resource form.
    /// </summary>
    public FormFieldProperties? Field =>
        ViewData[ViewDataKeys.FormFieldProperties] as FormFieldProperties;

    /// <summary>
    ///     Whether the field is read-only.
    /// </summary>
    public bool IsReadOnly => Field?.IsReadOnly == true || ViewData.ModelMetadata.IsReadOnly;

    /// <summary>
    ///     The field label, or null to use the property's display name.
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
