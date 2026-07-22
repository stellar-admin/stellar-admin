using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The region of an item that holds action controls such as buttons, aligned to its trailing edge.
/// </summary>
[HtmlTargetElement("sa-item-actions")]
public class ItemActionsTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-actions", GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
