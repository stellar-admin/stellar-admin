using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of a popover; typically contains the title and description.
/// </summary>
[HtmlTargetElement("sa-popover-header")]
public class PopoverHeaderTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-header");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-popover-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
