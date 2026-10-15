using System.ComponentModel;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     Renders field editors in index grid cells.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GridEditorHtmlHelperExtensions
{
    /// <summary>
    ///     Renders a field's editor for a grid cell, with the field's error under it.
    /// </summary>
    /// <remarks>
    ///     The editor's template resolves in <c>GridEditorTemplates/</c>, as a form's resolves in
    ///     <c>EditorTemplates/</c>. Without a grid template, the cell shows an error naming the
    ///     template in development and is empty otherwise.
    /// </remarks>
    /// <param name="html">The view's HTML helper, whose model holds the field.</param>
    /// <param name="field">The field to render.</param>
    public static Task<IHtmlContent> GridEditorAsync(this IHtmlHelper html, FormFieldOptions field)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(field);

        return GridEditorRenderer.RenderAsync(html, field);
    }
}
