using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a dropdown menu item that performs an action.
/// </summary>
/// <remarks>
///     The item renders as a button. Inside a form, an item with <c>type="submit"</c> submits it.
///     Use <c>sa-dropdown-menu-item-link</c> for navigation.
/// </remarks>
[HtmlTargetElement("sa-dropdown-menu-item")]
public class DropdownMenuItemTagHelper : StellarAdminTagHelperBase
{
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
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        // A button in a form submits it by default, so only an explicit type="submit" does
        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        DropdownMenuInternals.RenderItem(output, Variant, Inset, CloseOnClick, Disabled);
    }
}
