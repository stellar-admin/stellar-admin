using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the line below a message, which usually holds a timestamp, delivery status or
///     actions.
/// </summary>
[HtmlTargetElement("sa-message-footer")]
public class MessageFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "message-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-message-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
