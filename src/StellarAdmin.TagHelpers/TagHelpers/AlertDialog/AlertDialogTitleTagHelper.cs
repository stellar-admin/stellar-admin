using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The title heading of an alert dialog.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-title")]
public class AlertDialogTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-title");
        output.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-alert-dialog-title",
                "sa-font-heading",
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
