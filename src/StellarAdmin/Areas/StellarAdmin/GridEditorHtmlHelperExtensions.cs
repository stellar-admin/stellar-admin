using System.ComponentModel;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     Provides extension methods that render field editors in index grid cells.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GridEditorHtmlHelperExtensions
{
    /// <summary>
    ///     Renders a field's editor for a grid cell, with the field's error under it.
    /// </summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="field">The field to render.</param>
    /// <param name="model">The model that holds the field, such as the row's edit model.</param>
    /// <returns>A task that returns the rendered editor.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="html" />, <paramref name="field" /> or <paramref name="model" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The editor's template is found in <c>GridEditorTemplates/</c>, as a form's is found in
    ///     <c>EditorTemplates/</c>. Without a grid template, the cell shows an error that names the
    ///     template in development, and is empty otherwise.
    /// </remarks>
    public static Task<IHtmlContent> GridEditorAsync(
        this IHtmlHelper html,
        FormFieldOptions field,
        object model
    )
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(model);

        return FieldEditorRenderer.GuardAsync(
            html,
            field,
            () => GridEditorRenderer.RenderAsync(html, field, model)
        );
    }
}
