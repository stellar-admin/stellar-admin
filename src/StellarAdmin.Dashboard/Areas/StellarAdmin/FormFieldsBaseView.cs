using Microsoft.AspNetCore.Mvc.Razor;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

/// <summary>
///     The base class of the views that render a form's fields.
/// </summary>
/// <typeparam name="TModel">The type of the view's model.</typeparam>
public abstract class FormFieldsBaseView<TModel> : RazorPage<TModel>
{
    /// <summary>
    ///     All form fields as a flat list, including fields inside sections, groups, and rows, in display order.
    /// </summary>
    public IReadOnlyList<FormFieldOptions> Fields =>
        ViewData[ViewDataKeys.FormFields] as IReadOnlyList<FormFieldOptions> ?? [];

    /// <summary>
    ///     The fields and containers at the current layout level, preserving their nested structure for rendering.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items =>
        ViewData[ViewDataKeys.FormItems] as IReadOnlyList<FormItemOptions> ?? Fields;
}
