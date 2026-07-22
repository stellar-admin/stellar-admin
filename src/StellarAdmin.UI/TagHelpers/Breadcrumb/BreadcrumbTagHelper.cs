using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A breadcrumb navigation trail, rendered as a <c>&lt;nav&gt;</c>; shows the path to the
///     current page. Compose it with the list, item, link, page, separator, and ellipsis
///     subcomponents.
/// </summary>
[HtmlTargetElement("sa-breadcrumb")]
public class BreadcrumbTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("aria-label", "breadcrumb");
        output.Attributes.SetAttribute("data-slot", "breadcrumb");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
