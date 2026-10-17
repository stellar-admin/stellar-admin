using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A selectable menu item for an action. Renders a button, so inside a form an item with
///     <c>type="submit"</c> submits it. Use <c>sa-dropdown-menu-item-link</c> for navigation.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-item")]
public class DropdownMenuItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether clicking the item closes the menu. Plain items close on click unless this is set.
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
