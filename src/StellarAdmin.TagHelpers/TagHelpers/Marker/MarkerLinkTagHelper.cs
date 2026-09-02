using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A marker rendered as an anchor, making the whole marker a clickable link.
/// </summary>
[HtmlTargetElement("sa-marker-link")]
public class MarkerLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public MarkerLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     The layout of the marker.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="MarkerVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public MarkerVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? MarkerVariant.Default;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        MarkerRenderingHelper.Render(output, effectiveVariant);
    }
}
