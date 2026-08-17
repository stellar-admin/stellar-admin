using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

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

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        output.Attributes.SetAttribute("data-slot", "breadcrumb-link");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb-link", output.GetUserSuppliedClass())
        );
    }
}
