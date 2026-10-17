using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that makes the whole attachment clickable.
/// </summary>
/// <remarks>
///     The attachment's action buttons stay clickable.
/// </remarks>
[HtmlTargetElement("sa-attachment-button-trigger")]
public class AttachmentButtonTriggerTagHelper : StellarAdminTagHelperBase
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

        AttachmentTriggerRenderingHelper.Render(output);

        return Task.CompletedTask;
    }
}
