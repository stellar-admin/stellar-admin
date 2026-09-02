using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The column holding a message's header, message surface and footer.
/// </summary>
[HtmlTargetElement("sa-message-content")]
public class MessageContentTagHelper : StellarAdminTagHelperBase
{
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
