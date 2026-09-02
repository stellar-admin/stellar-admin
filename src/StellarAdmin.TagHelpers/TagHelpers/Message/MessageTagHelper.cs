using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A single message in a conversation, laying out its avatar, content, header and footer.
/// </summary>
[HtmlTargetElement("sa-message")]
public class MessageTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The side of the conversation the message sits on.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="MessageAlign.Start" />.
    /// </remarks>
    [HtmlAttributeName("align")]
    public MessageAlign? Align { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? MessageAlign.Start;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message");
        output.Attributes.SetAttribute("data-align", effectiveAlign.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message", "group/message", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
