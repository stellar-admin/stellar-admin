using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A control that scrolls a message scroller to the start or the end of its transcript.
/// </summary>
[HtmlTargetElement("sa-message-scroller-button")]
public class MessageScrollerButtonTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public MessageScrollerButtonTagHelper(IIconManager iconManager)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    /// <summary>
    ///     The end of the transcript the button scrolls to.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="MessageScrollerDirection.End" />.
    /// </remarks>
    [HtmlAttributeName("direction")]
    public MessageScrollerDirection? Direction { get; set; }

    /// <summary>
    ///     The size of the button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonSize.IconSmall" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     The visual style of the button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonVariant.Secondary" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveDirection = Direction ?? MessageScrollerDirection.End;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-slot", "message-scroller-button");
        output.Attributes.SetAttribute("data-direction", effectiveDirection.GetDataAttributeText());

        // Inert until the web component proves there is something to scroll to, so the button
        // stays hidden and unclickable when no script runs.
        if (!output.Attributes.ContainsName("data-active"))
        {
            output.Attributes.SetAttribute("data-active", "false");
        }

        // Target the parent `sel-message-scroller` via the native command API. When clicked,
        // the button dispatches a `command` event on that element, which scrolls it.
        var messageScrollerId = GetParentTagHelper<MessageScrollerTagHelper>()?.MessageScrollerId;
        if (messageScrollerId != null)
        {
            output.Attributes.SetAttribute(
                "command",
                effectiveDirection == MessageScrollerDirection.End
                    ? "--scroll-to-end"
                    : "--scroll-to-start"
            );
            output.Attributes.SetAttribute("commandfor", messageScrollerId);
        }

        ButtonRenderingHelper.RenderAttributes(
            output,
            Variant ?? ButtonVariant.Secondary,
            Size ?? ButtonSize.IconSmall
        );

        // RenderAttributes owns the class attribute, so the component's own class goes on in
        // front of it afterwards - the author's classes stay last either way.
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-message-scroller-button",
                output.Attributes["class"]?.Value?.ToString()
            )
        );

        var childContent = await output.GetChildContentAsync();
        if (childContent.IsEmptyOrWhiteSpace)
        {
            await RenderDefaultContentAsync(output, context, effectiveDirection);
        }
        else
        {
            output.Content.SetHtmlContent(childContent);
        }
    }

    private async Task RenderDefaultContentAsync(
        TagHelperOutput output,
        TagHelperContext context,
        MessageScrollerDirection direction
    )
    {
        // One arrow for both directions; the stylesheet rotates it for data-direction="start".
        var iconOutput = new TagHelperOutput(
            "svg",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconManager) { Name = "arrow-down" };
        await iconTagHelper.ProcessAsync(context, iconOutput);
        output.Content.AppendHtml(iconOutput);

        var label = direction == MessageScrollerDirection.End ? "Scroll to end" : "Scroll to start";
        output.Content.AppendHtml($"<span class=\"sr-only\">{label}</span>");
    }
}
