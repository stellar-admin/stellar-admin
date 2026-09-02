using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The avatar alongside a message, aligned with the bottom of the message.
/// </summary>
[HtmlTargetElement("sa-message-avatar")]
public class MessageAvatarTagHelper : StellarAdminTagHelperBase
{
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
