using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An attachment trigger rendered as a button, making the whole attachment activatable while
///     leaving its actions clickable.
/// </summary>
[HtmlTargetElement("sa-attachment-button-trigger")]
public class AttachmentButtonTriggerTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        AttachmentTriggerRenderingHelper.Render(output);

        return Task.CompletedTask;
    }
}
