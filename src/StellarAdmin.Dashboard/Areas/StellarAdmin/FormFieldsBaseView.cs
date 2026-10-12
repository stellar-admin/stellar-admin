using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

/// <summary>
///     The base class of the views that render a form's fields.
/// </summary>
/// <typeparam name="TModel">The type of the view's model.</typeparam>
public abstract class FormFieldsBaseView<TModel> : RazorPage<TModel>
{
    /// <summary>
    ///     All form fields as a flat list, including fields inside sections and groups, in display order.
    /// </summary>
    public IReadOnlyList<FormFieldOptions> Fields =>
        ViewData[ViewDataKeys.FormFields] as IReadOnlyList<FormFieldOptions> ?? [];

    /// <summary>
    ///     The fields and containers at the current layout level, preserving their nested structure for rendering.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items =>
        ViewData[ViewDataKeys.FormItems] as IReadOnlyList<FormItemOptions> ?? Fields;

    // Razor binds asp-for to an expression written in the view, so a field chosen at runtime is
    // rendered by pointing the page's model at the field's value, as an editor template sees it,
    // until the returned scope is disposed. The display template reads the field's display and its
    // prepared data from the view context's data, which the tag helper renders it with.
    internal IDisposable BeginField(FormDisplayFieldOptions field)
    {
        var fieldName = field.FieldName;
        var expression = Context
            .RequestServices.GetRequiredService<ModelExpressionProvider>()
            .CreateModelExpression(ViewData, fieldName);
        var original = ViewData;
        ViewData = new ViewDataDictionary<TModel>(original)
        {
            ModelExplorer = expression.ModelExplorer,
            TemplateInfo =
            {
                HtmlFieldPrefix = original.TemplateInfo.GetFullHtmlFieldName(fieldName),
            },
        };
        var contextData = ViewContext.ViewData;
        var originalDisplay = contextData[ViewDataKeys.FieldDisplay];
        var originalDisplayData = contextData[ViewDataKeys.FieldDisplayData];
        contextData[ViewDataKeys.FieldDisplay] = field.Display;
        contextData[ViewDataKeys.FieldDisplayData] = (
            original[ViewDataKeys.DisplayData]
            as IReadOnlyDictionary<FormDisplayFieldOptions, object?>
        )?.GetValueOrDefault(field);

        return new FieldScope(() =>
        {
            ViewData = original;
            contextData[ViewDataKeys.FieldDisplay] = originalDisplay;
            contextData[ViewDataKeys.FieldDisplayData] = originalDisplayData;
        });
    }

    private sealed class FieldScope(Action restore) : IDisposable
    {
        public void Dispose() => restore();
    }
}
