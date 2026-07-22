using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The descriptive body text of a dialog, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-dialog-description")]
public class DialogDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-description");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-dialog-description", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
