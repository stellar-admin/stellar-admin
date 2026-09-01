using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Bubble content rendered as a button, making the whole bubble activatable.
/// </summary>
[HtmlTargetElement("sa-bubble-button-content")]
public class BubbleButtonContentTagHelper : StellarAdminTagHelperBase
{
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
