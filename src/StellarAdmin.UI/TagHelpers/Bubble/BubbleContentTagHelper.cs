using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the framed content of a bubble, which holds the message text.
/// </summary>
[HtmlTargetElement("sa-bubble-content")]
public class BubbleContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        BubbleContentRenderingHelper.Render(output);

        return Task.CompletedTask;
    }
}
