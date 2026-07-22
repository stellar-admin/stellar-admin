using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The footer region of an item, spanning its full width beneath the main content.
/// </summary>
[HtmlTargetElement("sa-item-footer")]
public class ItemFooterTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-footer", GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
