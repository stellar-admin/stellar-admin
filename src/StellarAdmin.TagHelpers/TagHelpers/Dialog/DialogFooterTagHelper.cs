using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The footer region of a dialog; typically contains action buttons.
/// </summary>
[HtmlTargetElement("sa-dialog-footer")]
public class DialogFooterTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-footer");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-dialog-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
