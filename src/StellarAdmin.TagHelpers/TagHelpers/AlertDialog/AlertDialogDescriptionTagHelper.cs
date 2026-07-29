using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The descriptive body text of an alert dialog, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-description")]
public class AlertDialogDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-description");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-alert-dialog-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
