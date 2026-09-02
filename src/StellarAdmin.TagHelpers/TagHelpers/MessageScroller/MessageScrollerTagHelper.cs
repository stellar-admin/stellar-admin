using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A scrolling frame for a conversation, holding the transcript and its scroll controls.
/// </summary>
[HtmlTargetElement("sa-message-scroller")]
public class MessageScrollerTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether the transcript follows new content while the reader is at the newest message.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>. Following stops as soon as the reader scrolls away, and
    ///     resumes when they return to the newest message.
    /// </remarks>
    [HtmlAttributeName("auto-scroll")]
    public bool? AutoScroll { get; set; }

    /// <summary>
    ///     Where the transcript is scrolled to when it first appears.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="MessageScrollerPosition.End" />.
    /// </remarks>
    [HtmlAttributeName("initial-position")]
    public MessageScrollerPosition? InitialPosition { get; set; }

    /// <summary>
    ///     The id of the rendered element. Nested scroll buttons read this to target it with
    ///     <c>commandfor</c>.
    /// </summary>
    [HtmlAttributeNotBound]
    public string? MessageScrollerId { get; private set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAutoScroll = AutoScroll ?? true;
        var effectiveInitialPosition = InitialPosition ?? MessageScrollerPosition.End;

        // The host becomes <sel-message-scroller>, which owns the scroll behaviour. Both settings
        // are plain attributes rather than data-* because the web component reads them as its own
        // configuration; the transcript still lays out and scrolls without any script.
        output.TagName = "sel-message-scroller";
        output.TagMode = TagMode.StartTagAndEndTag;

        // Resolve the id before child content is processed so nested buttons can read it.
        // Honour a user-supplied id; otherwise generate a stable one.
        MessageScrollerId = output.Attributes.TryGetAttribute("id", out var idAttribute)
            ? idAttribute.Value.ToString()
            : null;
        if (MessageScrollerId == null)
        {
            MessageScrollerId = $"--sa-message-scroller-{GetUniqueId(context)}";
            output.Attributes.SetAttribute("id", MessageScrollerId);
        }

        output.Attributes.SetAttribute("data-slot", "message-scroller");
        output.Attributes.SetAttribute("auto-scroll", effectiveAutoScroll ? "true" : "false");
        output.Attributes.SetAttribute(
            "initial-position",
            effectiveInitialPosition.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-message-scroller",
                "group/message-scroller",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
