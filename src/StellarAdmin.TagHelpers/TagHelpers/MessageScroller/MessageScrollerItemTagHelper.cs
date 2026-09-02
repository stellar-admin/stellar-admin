using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A single row of a message scroller's transcript, wrapping one message.
/// </summary>
[HtmlTargetElement("sa-message-scroller-item")]
public class MessageScrollerItemTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-scroller-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-scroller-item", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
