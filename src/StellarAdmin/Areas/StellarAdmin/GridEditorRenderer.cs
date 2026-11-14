using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// Renders a field's editor in a grid cell. The editor's template name resolves in GridEditorTemplates/, as a form
// resolves it in EditorTemplates/: the handler's template for a field with an editor, or else the default for its
// property. A field without a grid template renders an error naming the template in development, and nothing in
// production.
internal static class GridEditorRenderer
{
    public static async Task<IHtmlContent> RenderAsync(
        IHtmlHelper html,
        FormFieldOptions field,
        object model
    )
    {
        var viewContext = html.ViewContext;
        var services = viewContext.HttpContext.RequestServices;
        var explorer = FieldEditorRenderer.CreateExplorer(
            html.MetadataProvider,
            field,
            model,
            services
        );

        // A field waiting for a field its items depend on has no items to load
        var waitingText = FieldDependencies.GetWaitingText(
            field,
            model,
            services,
            html.MetadataProvider
        );
        object? editorData = null;
        if (field.HandlerType is { } handlerType && waitingText is null)
        {
            var handler = (IFieldEditorHandler)
                ActivatorUtilities.CreateInstance(services, handlerType, field.Editor);
            var context = new FieldEditorContext(field.FieldName, model, explorer.Model);
            editorData = await handler.PrepareAsync(
                context,
                viewContext.HttpContext.RequestAborted
            );
        }

        var view = field.Editor is { } editor
            ? EditorTemplateFinder.FindView(
                viewContext,
                EditorTemplateFinder.GridFolder,
                editor.TemplateName
            )
            : EditorTemplateFinder
                .FindDefault(viewContext, EditorTemplateFinder.GridFolder, explorer.Metadata)
                ?.View;
        if (view is null)
        {
            return MissingTemplate(services, field, explorer.Metadata);
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
            IsBound = field.Binder is not null,
            IsDisabled = field.IsDisabled,
            IsReadOnly = field.IsReadOnly(model),
            Title = FieldLabels.Get(field, services, html.MetadataProvider),
            WaitingText = waitingText,
        };

        var content = new HtmlContentBuilder();
        await using (var writer = new StringWriter())
        {
            await view.RenderAsync(new ViewContext(viewContext, view, viewData, writer));
            content.AppendHtml(writer.ToString());
        }

        AppendError(content, viewData);
        if (html.ViewData[ViewDataKeys.GridCellErrors] is not HashSet<string> shown)
        {
            shown = new(StringComparer.OrdinalIgnoreCase);
            html.ViewData[ViewDataKeys.GridCellErrors] = shown;
        }

        shown.Add(viewData.TemplateInfo.HtmlFieldPrefix);

        return DependentFieldRenderer.Wrap(html, field, content, grid: true);
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

    private static IHtmlContent MissingTemplate(
        IServiceProvider services,
        FormFieldOptions field,
        ModelMetadata metadata
    )
    {
        if (!services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return HtmlString.Empty;
        }

        IEnumerable<string> templateNames = field.Editor is { } editor
            ? [editor.TemplateName]
            : EditorTemplateFinder.GetTemplateNames(metadata);
        var names = string.Join(
            ", ",
            templateNames.Select(name => $"{EditorTemplateFinder.GridFolder}/{name}")
        );
        var span = new TagBuilder("span");
        span.AddCssClass("sa-cell-error");
        span.Attributes["data-slot"] = "cell-editor-missing";
        span.InnerHtml.Append(
            $"The field '{field.FieldName}' has no grid editor template. Add one of: {names}."
        );

        return span;
    }
}
