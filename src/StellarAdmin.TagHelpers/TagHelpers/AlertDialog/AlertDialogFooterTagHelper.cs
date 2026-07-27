using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The footer region of an alert dialog; typically contains the cancel and action buttons.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-footer")]
public class AlertDialogFooterTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-footer");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-alert-dialog-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
