using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// A field whose items depend on other fields renders in admin-dependent-field, which names the field, its parents' inputs
// and labels, and the URL that renders it again when a parent changes. The script replaces the element with the one the
// URL returns for it.
internal static class DependentFieldRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IHtmlContent Wrap(
        IHtmlHelper html,
        FormFieldOptions field,
        IHtmlContent content,
        bool grid
    )
    {
        if (field.Dependencies.Count == 0)
        {
            return content;
        }

        var viewData = html.ViewData;
        var viewContext = html.ViewContext;
        var templateInfo = viewData.TemplateInfo;

        // The form is the one a lookup on the field would open its sheet for, and an edit form loads with its key
        var sheetLevel = viewData[ViewDataKeys.SheetLevel] as int?;
        var form = sheetLevel is not null
            ? "create"
            : viewData[ViewDataKeys.LookupForm] as string
                ?? viewContext.RouteData.Values["action"]?.ToString()?.ToLowerInvariant();
        var id = form == "edit" ? viewContext.RouteData.Values["id"] : null;
        var url = viewContext
            .HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>()
            .GetUrlHelper(viewContext);
        var parents = field
            .Dependencies.Where(dependency => dependency.Parent is not null)
            .Select(dependency => new
            {
                Name = templateInfo.GetFullHtmlFieldName(dependency.ParentFieldName),
                Field = dependency.ParentFieldName,
                Label = FieldLabels.Get(
                    dependency.Parent!,
                    viewContext.HttpContext.RequestServices,
                    html.MetadataProvider
                ),
            });

        var element = new TagBuilder("admin-dependent-field");
        element.AddCssClass("contents");
        element.Attributes["name"] = templateInfo.GetFullHtmlFieldName(field.FieldName);
        element.Attributes["parents"] = JsonSerializer.Serialize(parents, JsonOptions);
        element.Attributes["refresh-url"] = url.Action(
            "Dependents",
            new
            {
                id,
                form,
                prefix = templateInfo.HtmlFieldPrefix,
                level = sheetLevel,
                layout = grid ? "grid" : null,
            }
        );
        element.InnerHtml.AppendHtml(content);

        return element;
    }
}
