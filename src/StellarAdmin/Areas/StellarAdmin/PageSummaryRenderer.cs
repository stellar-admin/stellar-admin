using System.Collections;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// Renders a field of a page's record summary with the compact display template its index column would use, from the
// value read when the page loaded the record. An empty value shows the field's empty text, as on the view page.
internal static class PageSummaryRenderer
{
    private const string DefaultEmptyText = "—";

    public static async Task<IHtmlContent> RenderFieldAsync(
        ViewContext viewContext,
        PageSummaryFieldViewModel item
    )
    {
        var services = viewContext.HttpContext.RequestServices;
        var metadataProvider = services.GetRequiredService<IModelMetadataProvider>();
        var field = item.Field;
        var metadata = GetMetadata(field, metadataProvider);

        // A [UIHint] template wins over the default display, as on the view page
        var display =
            field.Display
            ?? (
                field.UsesBuiltInRendering || metadata.TemplateHint is not null
                    ? null
                    : FieldDisplayDefaults.Resolve(metadata)
            );
        var templateName = display?.TemplateName ?? metadata.TemplateHint;

        var element = new TagBuilder("span");
        element.Attributes["data-slot"] = "page-summary-field";
        element.AddCssClass("sa-page-summary-field");

        if (
            IsEmpty(item.Value) && !(display?.RendersEmptyValue == true && templateName is not null)
        )
        {
            element.Attributes["data-empty"] = "true";
            element.InnerHtml.Append(
                field.EmptyText ?? metadata.NullDisplayText ?? DefaultEmptyText
            );
        }
        else if (templateName is not null)
        {
            element.InnerHtml.AppendHtml(
                await RenderTemplateAsync(
                    viewContext,
                    templateName,
                    new ModelExplorer(metadataProvider, metadata, item.Value),
                    display,
                    item.DisplayData,
                    FieldLabels.Get(field, metadata)
                )
            );
        }
        else
        {
            var title = item.Value is { } value
                ? services.GetRequiredService<ResourceReferences>().FormatTitle(value)
                : null;
            element.InnerHtml.Append(title ?? FieldValueFormatter.Format(item.Value, metadata));
        }

        return element;
    }

    // The metadata of the field's property, as the grid resolves it property by property, or of a computed value's type
    public static ModelMetadata GetMetadata(
        FormDisplayFieldOptions field,
        IModelMetadataProvider metadataProvider
    )
    {
        if (field.PropertyPath.Count == 0)
        {
            return metadataProvider.GetMetadataForType(field.FieldExpression.ReturnType);
        }

        var containerType = field.FieldExpression.Parameters[0].Type;
        ModelMetadata? metadata = null;
        foreach (var property in field.PropertyPath)
        {
            metadata = metadataProvider.GetMetadataForProperty(containerType, property.Name);
            containerType = metadata.ModelType;
        }

        return metadata!;
    }

    public static bool IsEmpty(object? value) =>
        value switch
        {
            null => true,
            string text => string.IsNullOrWhiteSpace(text),
            IEnumerable items => !items.Cast<object?>().Any(),
            _ => false,
        };

    // The template reads its display, prepared data and label from the view context's data, as a grid cell's does
    public static async Task<IHtmlContent> RenderTemplateAsync(
        ViewContext viewContext,
        string templateName,
        ModelExplorer explorer,
        FieldDisplay? display,
        object? displayData,
        string label
    )
    {
        var viewName =
            templateName.StartsWith('/') || templateName.StartsWith("~/", StringComparison.Ordinal)
                ? templateName
                : $"{TemplateFinder.CompactDisplayFolder}/{templateName}";
        var result = TemplateFinder.Find(viewContext, viewName);
        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"The display template '{viewName}' was not found. The following locations were "
                    + $"searched: {string.Join(", ", result.SearchedLocations)}."
            );
        }

        var contextData = viewContext.ViewData;
        var originalDisplay = contextData[ViewDataKeys.FieldDisplay];
        var originalDisplayData = contextData[ViewDataKeys.FieldDisplayData];
        var originalLabel = contextData[ViewDataKeys.FieldLabel];
        contextData[ViewDataKeys.FieldDisplay] = display;
        contextData[ViewDataKeys.FieldDisplayData] = displayData;
        contextData[ViewDataKeys.FieldLabel] = label;
        try
        {
            var viewData = new ViewDataDictionary<object?>(contextData, explorer.Model)
            {
                ModelExplorer = explorer,
            };
            await using var writer = new StringWriter();
            await result.View.RenderAsync(
                new ViewContext(viewContext, result.View, viewData, writer)
            );

            return new HtmlString(writer.ToString());
        }
        finally
        {
            contextData[ViewDataKeys.FieldDisplay] = originalDisplay;
            contextData[ViewDataKeys.FieldDisplayData] = originalDisplayData;
            contextData[ViewDataKeys.FieldLabel] = originalLabel;
        }
    }
}
