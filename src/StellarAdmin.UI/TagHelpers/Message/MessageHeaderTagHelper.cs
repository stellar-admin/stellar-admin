using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the line above a message, which usually names the sender.
/// </summary>
[HtmlTargetElement("sa-message-header")]
public sealed class MessageHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
