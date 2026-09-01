using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A container that stacks consecutive bubbles from the same sender.
/// </summary>
[HtmlTargetElement("sa-bubble-group")]
public class BubbleGroupTagHelper : StellarAdminTagHelperBase
{
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
