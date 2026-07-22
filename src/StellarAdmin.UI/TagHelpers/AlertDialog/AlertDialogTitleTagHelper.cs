using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title heading of an alert dialog.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-title")]
public class AlertDialogTitleTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
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

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
