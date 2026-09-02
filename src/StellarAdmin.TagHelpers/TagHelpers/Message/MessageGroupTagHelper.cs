using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A container that stacks consecutive messages from the same sender.
/// </summary>
[HtmlTargetElement("sa-message-group")]
public class MessageGroupTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
