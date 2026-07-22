using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The footer region of a dialog; typically contains action buttons.
/// </summary>
[HtmlTargetElement("sa-dialog-footer")]
public class DialogFooterTagHelper : StellarAdminTagHelperBase
{
    public DialogFooterTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-footer");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge("sa-dialog-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
