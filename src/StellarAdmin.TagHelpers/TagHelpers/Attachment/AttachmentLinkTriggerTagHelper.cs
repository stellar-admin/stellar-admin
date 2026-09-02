using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An attachment trigger rendered as an anchor, making the whole attachment a clickable link
///     while leaving its actions clickable.
/// </summary>
[HtmlTargetElement("sa-attachment-link-trigger")]
public class AttachmentLinkTriggerTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public AttachmentLinkTriggerTagHelper(IHtmlGenerator htmlGenerator)
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
