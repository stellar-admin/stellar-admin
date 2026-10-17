using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     An attachment trigger rendered as an anchor, making the whole attachment a clickable link
///     while leaving its actions clickable.
/// </summary>
[HtmlTargetElement("sa-attachment-trigger-link")]
public class AttachmentTriggerLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public AttachmentTriggerLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        AttachmentTriggerRenderingHelper.Render(output);
    }
}
