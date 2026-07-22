using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A pagination link that navigates to the first page.
/// </summary>
[HtmlTargetElement("sa-pagination-first")]
public class PaginationFirstTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    /// <summary>
    ///     The size of the rendered pagination button.
    /// </summary>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    public PaginationFirstTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination-previous", output.GetUserSuppliedClass())
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
            // Render the icon
            var iconOutput = new TagHelperOutput(
                "svg",
                [],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconManager) { Name = "chevron-first" };
            await iconTagHelper.ProcessAsync(context, iconOutput);
            output.Content.AppendHtml(iconOutput);

            // Render the text
            var textBlockTagBuilder = new TagBuilder("span");
            textBlockTagBuilder.AddCssClass("sa-pagination-link-label");
            textBlockTagBuilder.InnerHtml.AppendHtml("First");
            output.Content.AppendHtml(textBlockTagBuilder);
        }
    }
}
