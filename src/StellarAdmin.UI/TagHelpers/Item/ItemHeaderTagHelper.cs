using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of an item, spanning its full width above the main content.
/// </summary>
[HtmlTargetElement("sa-item-header")]
public class ItemHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-header", GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
