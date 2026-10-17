using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the scrolling area of a message scroller, which holds the transcript.
/// </summary>
/// <remarks>
///     The area can take keyboard focus, so it can be scrolled with the keyboard. Its accessible
///     name is "Messages" unless the element sets <c>aria-label</c> or <c>aria-labelledby</c>.
/// </remarks>
[HtmlTargetElement("sa-message-scroller-viewport")]
public sealed class MessageScrollerViewportTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-scroller-viewport");

        if (!output.Attributes.ContainsName("role"))
        {
            output.Attributes.SetAttribute("role", "region");
        }

        // The region is focusable so a keyboard user can scroll the transcript without a
        // focusable message inside it.
        if (!output.Attributes.ContainsName("tabindex"))
        {
            output.Attributes.SetAttribute("tabindex", "0");
        }

        if (
            !output.Attributes.ContainsName("aria-label")
            && !output.Attributes.ContainsName("aria-labelledby")
        )
        {
            output.Attributes.SetAttribute("aria-label", "Messages");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-scroller-viewport", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
