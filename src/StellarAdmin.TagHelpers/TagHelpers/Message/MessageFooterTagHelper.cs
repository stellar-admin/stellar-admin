using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The line below a message, typically holding a timestamp, delivery status or message actions.
/// </summary>
[HtmlTargetElement("sa-message-footer")]
public class MessageFooterTagHelper : StellarAdminTagHelperBase
{
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
