using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A region within an alert dialog for media such as an icon or illustration.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-media")]
public class AlertDialogMediaTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-media");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-alert-dialog-media", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
