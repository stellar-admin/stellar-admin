using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Shared helpers for the Dropdown Menu family: content, item, indicator, shortcut and
///     separator rendering, also used by components that draw a menu of their own.
/// </summary>
internal static class DropdownMenuInternals
{
    // The shared popover-surface styling (positioning, overflow, open/close transition) lives in
    // Client/css/components/dropdown-menu.css under .sa-dropdown-menu-content /
    // .sa-dropdown-menu-sub-content.

    // Plain and link items share the menu item attributes; the menu activates items by role
    public static void RenderItem(
        TagHelperOutput output,
        DropdownMenuItemVariant? variant,
        bool? inset,
        bool? closeOnClick,
        bool? disabled
    )
    {
        output.Attributes.SetAttribute("role", "menuitem");
        output.Attributes.SetAttribute("tabindex", "-1");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-item");
        output.Attributes.SetAttribute(
            "data-variant",
            (variant ?? DropdownMenuItemVariant.Default).GetDataAttributeText()
        );
        if (inset == true)
        {
            output.Attributes.SetAttribute("data-inset", "true");
        }

        // Only emit when the author overrides the default (plain items close on click)
        if (closeOnClick.HasValue)
        {
            output.Attributes.SetAttribute(
                "data-close-on-click",
                closeOnClick.Value ? "true" : "false"
            );
        }

        if (disabled == true)
        {
            output.Attributes.SetAttribute("data-disabled", "");
            output.Attributes.SetAttribute("aria-disabled", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-dropdown-menu-item",
                output.GetUserSuppliedClass()
            )
        );
    }

    public static void RenderContent(
        TagHelperOutput output,
        DropdownMenuSide side,
        DropdownMenuAlign align
    )
    {
        output.TagName = "sel-dropdown-menu";
        output.TagMode = TagMode.StartTagAndEndTag;

        var sideText = side.GetDataAttributeText();
        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("role", "menu");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-content");
        output.Attributes.SetAttribute("data-anchor-side", sideText);
        output.Attributes.SetAttribute("data-side", sideText);
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-dropdown-menu-content",
                "sa-dropdown-menu-content-logical",
                AnchorPlacement.GetClassName(sideText, align.GetDataAttributeText()),
                output.GetUserSuppliedClass()
            )
        );
    }

    public static void RenderRadioItem(
        TagHelperOutput output,
        string? value,
        string? groupName,
        bool isChecked,
        bool? closeOnClick,
        bool? disabled
    )
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "menuitemradio");
        output.Attributes.SetAttribute("tabindex", "-1");
        output.Attributes.SetAttribute("aria-checked", isChecked ? "true" : "false");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-radio-item");
        output.Attributes.SetAttribute("data-state", isChecked ? "checked" : "unchecked");
        if (value != null)
        {
            output.Attributes.SetAttribute("data-value", value);
        }

        if (groupName != null)
        {
            output.Attributes.SetAttribute("data-radio-group", groupName);
        }

        // Only emit when the author overrides the default (radio items stay open on click).
        if (closeOnClick.HasValue)
        {
            output.Attributes.SetAttribute(
                "data-close-on-click",
                closeOnClick.Value ? "true" : "false"
            );
        }

        if (disabled == true)
        {
            output.Attributes.SetAttribute("data-disabled", "");
            output.Attributes.SetAttribute("aria-disabled", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-dropdown-menu-radio-item",
                output.GetUserSuppliedClass()
            )
        );
    }

    // The check mark that shows when a checkbox or radio item is checked; slot names its item kind
    public static TagBuilder ItemIndicator(IconOptions iconOptions, string slot)
    {
        var indicator = new TagBuilder("span");
        indicator.Attributes["data-slot"] = slot;
        indicator.Attributes["class"] = "sa-dropdown-menu-item-indicator";
        indicator.InnerHtml.AppendHtml(
            iconOptions.RenderIcon(SemanticIconRole.MenuItemSelected, ("class", "size-4"))
        );

        return indicator;
    }

    public static void RenderShortcut(TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dropdown-menu-shortcut");
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-dropdown-menu-shortcut",
                output.GetUserSuppliedClass()
            )
        );
    }

    public static void RenderSeparator(TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute("aria-orientation", "horizontal");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-separator");
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-dropdown-menu-separator",
                output.GetUserSuppliedClass()
            )
        );
    }
}
