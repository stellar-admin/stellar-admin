using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the content of a bubble as a button, so the whole bubble can be clicked.
/// </summary>
[HtmlTargetElement("sa-bubble-button-content")]
public class BubbleButtonContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        BubbleContentRenderingHelper.Render(output);

        return Task.CompletedTask;
    }
}
