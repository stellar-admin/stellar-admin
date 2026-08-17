using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An item rendered as an anchor, making the entire row a clickable link.
/// </summary>
[HtmlTargetElement("sa-link-item")]
public class LinkItemTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     The size of the item, controlling its padding and spacing.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ItemSize.Default" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public ItemSize? Size { get; set; }

    /// <summary>
    ///     The visual style of the item.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ItemVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ItemVariant? Variant { get; set; }

    public LinkItemTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        ItemRenderingHelper.Render(output, Size, Variant);
    }
}
