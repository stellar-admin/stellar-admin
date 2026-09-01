using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The framed surface of a bubble, holding the message text.
/// </summary>
[HtmlTargetElement("sa-bubble-content")]
public class BubbleContentTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        BubbleContentRenderingHelper.Render(output);

        return Task.CompletedTask;
    }
}
