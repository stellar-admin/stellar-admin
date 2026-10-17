using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the avatar beside a message, aligned with the bottom of the message.
/// </summary>
[HtmlTargetElement("sa-message-avatar")]
public sealed class MessageAvatarTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-avatar");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-avatar", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
