using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Navigation for moving between pages of content.
/// </summary>
[HtmlTargetElement("sa-pagination")]
public class PaginationTagHelper : StellarAdminTagHelperBase
{
    public PaginationTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "navigation");
        output.Attributes.SetAttribute("aria-label", "pagination");
        output.Attributes.SetAttribute("data-slot", "pagination");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-pagination", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
