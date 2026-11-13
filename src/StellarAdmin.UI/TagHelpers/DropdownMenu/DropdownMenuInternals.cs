using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Shared helpers for the Dropdown Menu family: item rendering.
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
}
