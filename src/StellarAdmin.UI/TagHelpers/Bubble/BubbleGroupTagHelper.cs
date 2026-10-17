using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a stack of consecutive bubbles from the same sender.
/// </summary>
[HtmlTargetElement("sa-bubble-group")]
public class BubbleGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "bubble-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-bubble-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
