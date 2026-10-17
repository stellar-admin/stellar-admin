using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Bubble content rendered as an anchor, making the whole bubble a clickable link.
/// </summary>
[HtmlTargetElement("sa-bubble-content-link")]
public class BubbleContentLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public BubbleContentLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        BubbleContentRenderingHelper.Render(output);
    }
}
