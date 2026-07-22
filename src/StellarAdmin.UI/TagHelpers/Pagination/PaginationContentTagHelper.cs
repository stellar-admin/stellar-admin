using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The list that holds the individual pagination items.
/// </summary>
[HtmlTargetElement("sa-pagination-content")]
public class PaginationContentTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "pagination-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
