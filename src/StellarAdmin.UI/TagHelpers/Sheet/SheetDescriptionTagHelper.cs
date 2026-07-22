using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Supporting description text for a sheet, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-sheet-description")]
public class SheetDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "sheet-description");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-sheet-description", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
