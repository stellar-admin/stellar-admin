using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a scrolling frame for a conversation, with its transcript and scroll buttons.
/// </summary>
/// <remarks>
///     Requires <c>stellar-admin.js</c> for automatic scrolling and the scroll buttons. Without it,
///     the transcript still scrolls.
/// </remarks>
[HtmlTargetElement("sa-message-scroller")]
public sealed class MessageScrollerTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value that indicates whether the transcript follows new messages.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the newest message stays in view while the reader is at the
    ///     end of the transcript; otherwise, <see langword="false" />. The default is
    ///     <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Following stops when the reader scrolls away and resumes when they return to the newest
    ///     message.
    /// </remarks>
    [HtmlAttributeName("auto-scroll")]
    public bool? AutoScroll { get; set; }

    /// <summary>
    ///     Gets or sets where the transcript is scrolled to when it first appears.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the starting position. The default is
    ///     <see cref="MessageScrollerPosition.End" />.
    /// </value>
    [HtmlAttributeName("initial-position")]
    public MessageScrollerPosition? InitialPosition { get; set; }

    internal string? MessageScrollerId { get; private set; }

    /// <inheritdoc />
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
            JoinCssClasses("sa-message-scroller", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
