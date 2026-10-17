using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the transcript of a message scroller, which stacks its messages.
/// </summary>
/// <remarks>
///     Screen readers announce new messages as they are added.
/// </remarks>
[HtmlTargetElement("sa-message-scroller-content")]
public class MessageScrollerContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-scroller-content");

        // Announces messages as they arrive, without re-reading the transcript already on screen.
        if (!output.Attributes.ContainsName("role"))
        {
            output.Attributes.SetAttribute("role", "log");
        }

        if (!output.Attributes.ContainsName("aria-relevant"))
        {
            output.Attributes.SetAttribute("aria-relevant", "additions");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-scroller-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
