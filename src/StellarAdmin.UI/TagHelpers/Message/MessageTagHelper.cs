using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a message in a conversation, with its avatar, content, header and footer.
/// </summary>
[HtmlTargetElement("sa-message")]
public sealed class MessageTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the side of the conversation that the message sits on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the side. The default is
    ///     <see cref="MessageAlign.Start" />.
    /// </value>
    [HtmlAttributeName("align")]
    public MessageAlign? Align { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? MessageAlign.Start;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message");
        output.Attributes.SetAttribute("data-align", effectiveAlign.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
