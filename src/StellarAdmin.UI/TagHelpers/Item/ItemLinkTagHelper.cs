using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an item as a link, so the whole row can be clicked.
/// </summary>
[HtmlTargetElement("sa-item-link")]
public class ItemLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Gets or sets the size of the item, which sets its padding and spacing.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size of the item. The default is
    ///     <see cref="ItemSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ItemSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the item.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="ItemVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ItemVariant? Variant { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that builds the link URL from the routing attributes.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public ItemLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        ItemRenderingHelper.Render(output, Size, Variant);
    }
}
