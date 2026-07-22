using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The list that holds the individual pagination items.
/// </summary>
[HtmlTargetElement("sa-pagination-content")]
public class PaginationContentTagHelper : StellarAdminTagHelperBase
{
    public PaginationContentTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "pagination-content");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-pagination-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
