using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the area of an item that holds actions, such as buttons.
/// </summary>
[HtmlTargetElement("sa-item-actions")]
public class ItemActionsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
