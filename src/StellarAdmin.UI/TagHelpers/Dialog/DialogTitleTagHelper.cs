using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title heading of a dialog.
/// </summary>
[HtmlTargetElement("sa-dialog-title")]
public class DialogTitleTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-title");
        output.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-dialog-title",
                "sa-font-heading",
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
