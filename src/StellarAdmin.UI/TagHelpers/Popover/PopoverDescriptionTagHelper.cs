using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The descriptive body text of a popover, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-popover-description")]
public class PopoverDescriptionTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-description");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-popover-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
