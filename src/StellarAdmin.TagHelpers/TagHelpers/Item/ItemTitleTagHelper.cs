using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The primary title text of an item.
/// </summary>
[HtmlTargetElement("sa-item-title")]
public class ItemTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-title", GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
