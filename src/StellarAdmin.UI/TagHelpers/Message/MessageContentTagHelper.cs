using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the column that holds a message's header, body and footer.
/// </summary>
[HtmlTargetElement("sa-message-content")]
public sealed class MessageContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
