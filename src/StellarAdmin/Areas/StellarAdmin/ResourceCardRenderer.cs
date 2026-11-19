using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// Renders a card's badge, stats and fields with the compact display template a table cell would use, as the page
// summary does. Each value comes with its text, which the card shows in full on hover where the value is cut off
internal static partial class ResourceCardRenderer
{
    private const string DefaultEmptyText = "—";

    public static string GetLabel(ViewContext viewContext, FormDisplayFieldOptions field) =>
        FieldLabels.Get(field, GetMetadata(viewContext, field));

    public static async Task<ResourceCardValue> RenderAsync(
        ViewContext viewContext,
        FormDisplayFieldOptions field,
        object item,
        FieldDisplay? defaultDisplay = null
    )
    {
        var services = viewContext.HttpContext.RequestServices;
        var metadataProvider = services.GetRequiredService<IModelMetadataProvider>();
        var metadata = GetMetadata(viewContext, field);
        var value = field.GetValue(item);

        // A [UIHint] template wins over the default display, as in a table cell
        var display =
            field.Display
            ?? defaultDisplay
            ?? (
                field.UsesBuiltInRendering || metadata.TemplateHint is not null
                    ? null
                    : FieldDisplayDefaults.Resolve(metadata)
            );
        var templateName = display?.TemplateName ?? metadata.TemplateHint;

        if (
            PageSummaryRenderer.IsEmpty(value)
            && !(display?.RendersEmptyValue == true && templateName is not null)
        )
        {
            var empty = field.EmptyText ?? metadata.NullDisplayText ?? DefaultEmptyText;

            return new(new HtmlString(WebUtility.HtmlEncode(empty)), empty, true);
        }

        if (templateName is not null)
        {
            var html = await PageSummaryRenderer.RenderTemplateAsync(
                viewContext,
                templateName,
                new ModelExplorer(metadataProvider, metadata, value),
                display,
                null,
                FieldLabels.Get(field, metadata)
            );

            return new(html, GetText(html), false);
        }

        var text =
            (
                value is null
                    ? null
                    : services.GetRequiredService<ResourceReferences>().FormatTitle(value)
            ) ?? FieldValueFormatter.Format(value, metadata);

        return new(new HtmlString(WebUtility.HtmlEncode(text)), text, false);
    }

    private static ModelMetadata GetMetadata(
        ViewContext viewContext,
        FormDisplayFieldOptions field
    ) =>
        PageSummaryRenderer.GetMetadata(
            field,
            viewContext.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>()
        );

    // The text a reader sees in the rendered value, on one line
    private static string GetText(IHtmlContent html)
    {
        using var writer = new StringWriter();
        html.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
        var text = WebUtility.HtmlDecode(Tags().Replace(writer.ToString(), " "));

        return Whitespace().Replace(text, " ").Trim();
    }

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex Tags();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}

// A card's value as it renders, and as text
internal sealed record ResourceCardValue(IHtmlContent Html, string Text, bool IsEmpty);
