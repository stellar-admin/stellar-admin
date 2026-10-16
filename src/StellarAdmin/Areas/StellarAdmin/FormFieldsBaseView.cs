using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

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
    internal FieldScope BeginField(FormDisplayFieldOptions field)
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

        // A [UIHint] template wins over the default display, and the tag helper renders it
        var metadata = expression.Metadata;
        var display =
            field.Display
            ?? (
                field.UsesBuiltInRendering || metadata.TemplateHint is not null
                    ? null
                    : FieldDisplayDefaults.Resolve(metadata)
            );
        var contextData = ViewContext.ViewData;
        var originalDisplay = contextData[ViewDataKeys.FieldDisplay];
        var originalDisplayData = contextData[ViewDataKeys.FieldDisplayData];
        contextData[ViewDataKeys.FieldDisplay] = display;
        contextData[ViewDataKeys.FieldDisplayData] = (
            original[ViewDataKeys.DisplayData]
            as IReadOnlyDictionary<FormDisplayFieldOptions, object?>
        )?.GetValueOrDefault(field);

        return new FieldScope(
            display?.TemplateName,
            display?.RendersEmptyValue == true,
            () =>
            {
                ViewData = original;
                contextData[ViewDataKeys.FieldDisplay] = originalDisplay;
                contextData[ViewDataKeys.FieldDisplayData] = originalDisplayData;
            }
        );
    }

    internal sealed class FieldScope(
        string? templateName,
        bool templateRendersEmpty,
        Action restore
    ) : IDisposable
    {
        // The display template the field renders, or null to let the tag helper choose
        public string? TemplateName { get; } = templateName;

        // Whether the template renders an empty value instead of the field's empty text
        public bool TemplateRendersEmpty { get; } = templateRendersEmpty;

        public void Dispose() => restore();
    }
}
