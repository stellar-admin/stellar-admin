using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A pagination link that navigates to the last page.
/// </summary>
[HtmlTargetElement("sa-pagination-last")]
public class PaginationLastTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    /// <summary>
    ///     The size of the rendered pagination button.
    /// </summary>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    public PaginationLastTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
    {
        _htmlGenerator = htmlGenerator;
        _iconManager = iconManager;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination-next", output.GetUserSuppliedClass())
        );
        var linkTagHelper = new PaginationLinkTagHelper(_htmlGenerator)
        {
            ViewContext = ViewContext,
            Action = Action,
            Area = Area,
            Controller = Controller,
            Fragment = Fragment,
            Host = Host,
            Page = Page,
            PageHandler = PageHandler,
            Protocol = Protocol,
            Route = Route,
            RouteValues = RouteValues,
            IsActive = false,
            Size = Size,
        };
        await linkTagHelper.ProcessAsync(context, output);

        var content = await output.GetChildContentAsync();

        if (!content.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(content);
        }
        else
        {
            // Render the text
            var textBlockTagBuilder = new TagBuilder("span");
            textBlockTagBuilder.AddCssClass("sa-pagination-link-label");
            textBlockTagBuilder.InnerHtml.AppendHtml("Last");
            output.Content.AppendHtml(textBlockTagBuilder);

            // Render the icon
            var iconOutput = new TagHelperOutput(
                "svg",
                [],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconManager) { Name = "chevron-last" };
            await iconTagHelper.ProcessAsync(context, iconOutput);
            output.Content.AppendHtml(iconOutput);
        }
    }
}
