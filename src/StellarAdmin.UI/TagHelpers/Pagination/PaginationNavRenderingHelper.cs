using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

internal static class PaginationNavRenderingHelper
{
    public static PaginationLinkTagHelper CreateLink(
        IHtmlGenerator htmlGenerator,
        StellarAdminAnchorTagHelperBase source,
        ButtonSize? size
    )
    {
        return new PaginationLinkTagHelper(htmlGenerator)
        {
            ViewContext = source.ViewContext,
            Action = source.Action,
            Area = source.Area,
            Controller = source.Controller,
            Fragment = source.Fragment,
            Host = source.Host,
            Page = source.Page,
            PageHandler = source.PageHandler,
            Protocol = source.Protocol,
            Route = source.Route,
            RouteValues = source.RouteValues,
            Active = false,
            Size = size,
        };
    }

    // Without child content, the link shows the direction's icon and label. Compact (in-grid)
    // links are icon-only, so they carry an aria-label instead of the label.
    public static async Task RenderAsync(
        TagHelperContext context,
        TagHelperOutput output,
        PaginationLinkTagHelper link,
        PaginationNavDirection direction,
        IconOptions iconOptions,
        bool compact = false
    )
    {
        var (label, iconRole) = direction switch
        {
            PaginationNavDirection.First => ("First", SemanticIconRole.PaginationFirst),
            PaginationNavDirection.Previous => ("Previous", SemanticIconRole.PaginationPrevious),
            PaginationNavDirection.Next => ("Next", SemanticIconRole.PaginationNext),
            _ => ("Last", SemanticIconRole.PaginationLast),
        };
        var iconFirst =
            direction is PaginationNavDirection.First or PaginationNavDirection.Previous;

        // First and Previous share the class that pads the icon side, as do Next and Last.
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                iconFirst ? "sa-pagination-previous" : "sa-pagination-next",
                output.GetUserSuppliedClass()
            )
        );
        if (compact)
        {
            output.Attributes.SetAttribute("aria-label", $"Go to {label.ToLowerInvariant()} page");
        }

        await link.ProcessAsync(context, output);

        var content = await output.GetChildContentAsync();
        if (!content.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(content);
            return;
        }

        var icon = iconOptions.RenderIcon(iconRole);
        if (iconFirst)
        {
            output.Content.AppendHtml(icon);
        }

        if (!compact)
        {
            var labelBuilder = new TagBuilder("span");
            labelBuilder.AddCssClass("sa-pagination-link-label");
            labelBuilder.InnerHtml.Append(label);
            output.Content.AppendHtml(labelBuilder);
        }

        if (!iconFirst)
        {
            output.Content.AppendHtml(icon);
        }
    }
}
