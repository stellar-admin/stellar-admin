using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// Renders a field's editor in a grid cell. The editor's template name resolves in GridEditorTemplates/, as a form
// resolves it in EditorTemplates/: the handler's template for a field with an editor, or else the names MVC derives
// from the property's [UIHint], [DataType] and type. A field without a grid template renders an error naming the
// template in development, and nothing in production.
internal static class GridEditorRenderer
{
    private const string Folder = "GridEditorTemplates";

    public static async Task<IHtmlContent> RenderAsync(IHtmlHelper html, FormFieldOptions field)
    {
        var viewContext = html.ViewContext;
        var services = viewContext.HttpContext.RequestServices;
        var explorer = html.ViewData.ModelExplorer;
        foreach (var property in field.PropertyPath)
        {
            explorer =
                explorer.GetExplorerForProperty(property.Name)
                ?? throw new InvalidOperationException(
                    $"{explorer.ModelType.Name} has no property {property.Name} for the field '{field.FieldName}'."
                );
        }

        object? editorData = null;
        string[] templateNames;
        if (field.HandlerType is { } handlerType)
        {
            var handler = (IFieldEditorHandler)
                ActivatorUtilities.CreateInstance(services, handlerType, field.Editor);
            var model = html.ViewData.Model!;
            var context = new FieldEditorContext(
                field.FieldName,
                model,
                ResourcePropertyPath.GetValue(model, field.PropertyPath)
            );
            editorData = await handler.PrepareAsync(
                context,
                viewContext.HttpContext.RequestAborted
            );
            templateNames = [handler.TemplateName];
        }
        else
        {
            templateNames = GetTemplateNames(explorer.Metadata).ToArray();
        }

        var view = FindView(services, viewContext, templateNames);
        if (view is null)
        {
            return MissingTemplate(services, field, templateNames);
        }

        var viewData = new ViewDataDictionary(html.ViewData) { ModelExplorer = explorer };
        viewData.TemplateInfo.HtmlFieldPrefix = html.ViewData.TemplateInfo.GetFullHtmlFieldName(
            field.FieldName
        );
        viewData.TemplateInfo.FormattedModelValue = explorer.Model;
        viewData[ViewDataKeys.FormFieldProperties] = new FormFieldProperties
        {
            Description = field.Description,
            EditorData = editorData,
            Editor = field.Editor,
            FieldName = field.FieldName,
            IsReadOnly = field.IsReadOnly,
            Title = field.Title,
        };

        var content = new HtmlContentBuilder();
        await using (var writer = new StringWriter())
        {
            await view.RenderAsync(new ViewContext(viewContext, view, viewData, writer));
            content.AppendHtml(writer.ToString());
        }

        AppendError(content, viewData);

        return content;
    }

    // The id of the error under a cell's editor, while the field has one
    public static string? GetErrorId(ViewDataDictionary viewData)
    {
        var name = viewData.TemplateInfo.HtmlFieldPrefix;

        return viewData.ModelState.GetFieldValidationState(name) == ModelValidationState.Invalid
            ? $"{TagBuilder.CreateSanitizedId(name, "_")}-error"
            : null;
    }

    private static void AppendError(HtmlContentBuilder content, ViewDataDictionary viewData)
    {
        if (
            GetErrorId(viewData) is not { } errorId
            || viewData.ModelState[viewData.TemplateInfo.HtmlFieldPrefix]?.Errors
                is not [var error, ..]
        )
        {
            return;
        }

        var span = new TagBuilder("span");
        span.AddCssClass("sa-cell-error");
        span.Attributes["id"] = errorId;
        span.Attributes["data-slot"] = "cell-error";
        span.InnerHtml.Append(
            string.IsNullOrEmpty(error.ErrorMessage) ? "The value is invalid." : error.ErrorMessage
        );
        content.AppendHtml(span);
    }

    private static IView? FindView(
        IServiceProvider services,
        ViewContext viewContext,
        IEnumerable<string> templateNames
    )
    {
        var viewEngine = services.GetRequiredService<ICompositeViewEngine>();
        foreach (var templateName in templateNames)
        {
            var viewName = $"{Folder}/{templateName}";
            var result = viewEngine.GetView(viewContext.ExecutingFilePath, viewName, false);
            if (!result.Success)
            {
                result = viewEngine.FindView(viewContext, viewName, false);
            }

            if (result.Success)
            {
                return result.View;
            }
        }

        return null;
    }

    // The names MVC tries for an editor template without a template name, most specific first
    private static IEnumerable<string> GetTemplateNames(ModelMetadata metadata)
    {
        if (!string.IsNullOrEmpty(metadata.TemplateHint))
        {
            yield return metadata.TemplateHint;
        }

        if (!string.IsNullOrEmpty(metadata.DataTypeName))
        {
            yield return metadata.DataTypeName;
        }

        var type = metadata.UnderlyingOrModelType;
        yield return type.Name;
        if (type == typeof(string) || metadata.IsComplexType)
        {
            yield break;
        }

        if (type.IsEnum)
        {
            yield return "Enum";
        }
        else if (type == typeof(DateTimeOffset))
        {
            yield return "DateTime";
        }

        yield return "String";
    }

    private static IHtmlContent MissingTemplate(
        IServiceProvider services,
        FormFieldOptions field,
        IReadOnlyList<string> templateNames
    )
    {
        if (!services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return HtmlString.Empty;
        }

        var names = string.Join(", ", templateNames.Select(name => $"{Folder}/{name}"));
        var span = new TagBuilder("span");
        span.AddCssClass("sa-cell-error");
        span.Attributes["data-slot"] = "cell-editor-missing";
        span.InnerHtml.Append(
            $"The field '{field.FieldName}' has no grid editor template. Add one of: {names}."
        );

        return span;
    }
}
