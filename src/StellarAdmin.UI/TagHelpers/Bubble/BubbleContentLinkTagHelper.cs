using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the content of a bubble as a link, so the whole bubble can be clicked.
/// </summary>
/// <remarks>
///     Set <c>href</c> or the <c>asp-*</c> routing attributes.
/// </remarks>
[HtmlTargetElement("sa-bubble-content-link")]
public class BubbleContentLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BubbleContentLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that builds the link's URL from the routing attributes.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public BubbleContentLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        BubbleContentRenderingHelper.Render(output);
    }
}
