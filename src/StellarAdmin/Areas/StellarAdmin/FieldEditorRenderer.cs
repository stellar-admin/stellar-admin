using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// A field with a binder edits the binder's value, such as a list of keys, in place of its property's. Its template
// renders that value with the property's metadata, so it keeps the property's label, description and validation.
internal static class FieldEditorRenderer
{
    public static ModelExplorer CreateExplorer(
        IModelMetadataProvider metadataProvider,
        FormFieldOptions field,
        object model,
        IServiceProvider services
    )
    {
        var explorer = metadataProvider.GetModelExplorerForType(model.GetType(), model);
        foreach (var property in field.PropertyPath)
        {
            explorer =
                explorer.GetExplorerForProperty(property.Name)
                ?? throw new InvalidOperationException(
                    $"{explorer.ModelType.Name} has no property {property.Name} for the field '{field.FieldName}'."
                );
        }

        return field.Binder is null
            ? explorer
            : explorer.Container.GetExplorerForExpression(
                explorer.Metadata,
                field.GetValue(model, services)
            );
    }

    // A form's editor for a field with a binder, as Html.Editor renders the others
    public static IHtmlContent RenderFormEditor(
        IHtmlHelper html,
        FormFieldOptions field,
        object model,
        string? templateName,
        object additionalViewData
    )
    {
        var viewContext = html.ViewContext;
        var services = viewContext.HttpContext.RequestServices;
        var viewData = new ViewDataDictionary(html.ViewData)
        {
            ModelExplorer = CreateExplorer(html.MetadataProvider, field, model, services),
        };
        viewData.TemplateInfo.HtmlFieldPrefix = html.ViewData.TemplateInfo.GetFullHtmlFieldName(
            field.FieldName
        );

        var helper = services.GetRequiredService<IHtmlHelper>();
        ((IViewContextAware)helper).Contextualize(
            new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer)
        );

        return helper.Editor(string.Empty, templateName, additionalViewData);
    }
}
