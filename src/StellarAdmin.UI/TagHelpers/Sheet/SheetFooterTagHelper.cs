using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The footer region of a sheet; typically contains action buttons.
/// </summary>
[HtmlTargetElement("sa-sheet-footer")]
public class SheetFooterTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-footer");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-sheet-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
