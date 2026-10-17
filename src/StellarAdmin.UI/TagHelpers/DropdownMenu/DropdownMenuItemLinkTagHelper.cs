using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A menu item rendered as a link, navigating to its target when chosen.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-item-link")]
public class DropdownMenuItemLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public DropdownMenuItemLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Whether clicking the item closes the menu. Link items close on click unless this is set.
    /// </summary>
    [HtmlAttributeName("close-on-click")]
    public bool? CloseOnClick { get; set; }

    /// <summary>
    ///     Whether the item is disabled.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Whether the item is inset, aligning its text with items that have a leading icon.
    /// </summary>
    [HtmlAttributeName("inset")]
    public bool? Inset { get; set; }

    /// <summary>
    ///     The visual style of the item.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DropdownMenuItemVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public DropdownMenuItemVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        DropdownMenuInternals.RenderItem(output, Variant, Inset, CloseOnClick, Disabled);
    }
}
