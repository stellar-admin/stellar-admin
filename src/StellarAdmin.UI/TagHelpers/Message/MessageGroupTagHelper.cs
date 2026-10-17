using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of consecutive messages from the same sender.
/// </summary>
[HtmlTargetElement("sa-message-group")]
public sealed class MessageGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
