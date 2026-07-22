using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of an alert dialog; typically contains the title and description.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-header")]
public class AlertDialogHeaderTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-header");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-alert-dialog-header", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
