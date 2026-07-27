using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The header region of a sheet; typically contains the title and description.
/// </summary>
[HtmlTargetElement("sa-sheet-header")]
public class SheetHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sheet-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
