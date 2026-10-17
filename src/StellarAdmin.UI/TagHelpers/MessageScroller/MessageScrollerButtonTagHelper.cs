using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that scrolls a message scroller to the start or end of its transcript.
/// </summary>
/// <remarks>
///     The button stays hidden until there is something to scroll to. With no content, it shows an
///     arrow icon and a screen reader label. Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-message-scroller-button")]
public sealed class MessageScrollerButtonTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageScrollerButtonTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the default arrow icon.</param>
    public MessageScrollerButtonTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the end of the transcript that the button scrolls to.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the end of the transcript. The default is
    ///     <see cref="MessageScrollerDirection.End" />.
    /// </value>
    [HtmlAttributeName("direction")]
    public MessageScrollerDirection? Direction { get; set; }

    /// <summary>
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ButtonSize.IconSmall" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="ButtonVariant.Secondary" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
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
        var iconTagHelper = new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(SemanticIconRole.ScrollToEnd),
        };
        await iconTagHelper.ProcessAsync(context, iconOutput);
        output.Content.AppendHtml(iconOutput);

        var label = direction == MessageScrollerDirection.End ? "Scroll to end" : "Scroll to start";
        output.Content.AppendHtml($"<span class=\"sr-only\">{label}</span>");
    }
}
