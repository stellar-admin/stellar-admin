using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Navigation for moving between pages of content.
/// </summary>
[HtmlTargetElement("sa-pagination")]
public class PaginationTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "navigation");
        output.Attributes.SetAttribute("aria-label", "pagination");
        output.Attributes.SetAttribute("data-slot", "pagination");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
