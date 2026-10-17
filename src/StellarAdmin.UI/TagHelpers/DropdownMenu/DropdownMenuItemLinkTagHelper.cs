using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a dropdown menu item that links to another page.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-item-link")]
public class DropdownMenuItemLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DropdownMenuItemLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the link's URL.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public DropdownMenuItemLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether clicking the item closes the menu.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if clicking the item closes the menu; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    [HtmlAttributeName("close-on-click")]
    public bool? CloseOnClick { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item is disabled and can't be chosen; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is inset.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item's text aligns with items that have a leading icon;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("inset")]
    public bool? Inset { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the item.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="DropdownMenuItemVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public DropdownMenuItemVariant? Variant { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        DropdownMenuInternals.RenderItem(output, Variant, Inset, CloseOnClick, Disabled);
    }
}
