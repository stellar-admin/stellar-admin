using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The footer region of a sheet; typically contains action buttons.
/// </summary>
[HtmlTargetElement("sa-sheet-footer")]
public class SheetFooterTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sheet-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
