using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The line above a message, typically naming the sender.
/// </summary>
[HtmlTargetElement("sa-message-header")]
public class MessageHeaderTagHelper : StellarAdminTagHelperBase
{
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
