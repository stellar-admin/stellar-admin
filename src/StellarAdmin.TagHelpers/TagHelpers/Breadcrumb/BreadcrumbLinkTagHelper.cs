using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkAnchorTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A navigable link within a breadcrumb item, rendered as an <c>&lt;a&gt;</c>; supports
///     the standard anchor routing attributes.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-link")]
public class BreadcrumbLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public BreadcrumbLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        var anchorTagHelper = new FrameworkAnchorTagHelper(_htmlGenerator)
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
        };
        await anchorTagHelper.ProcessAsync(context, output);

        output.Attributes.SetAttribute("data-slot", "breadcrumb-link");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb-link", output.GetUserSuppliedClass())
        );
    }
}
