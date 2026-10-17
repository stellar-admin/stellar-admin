using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Shared helpers for the Dropdown Menu family: indicator/chevron icon and item rendering.
/// </summary>
internal static class DropdownMenuInternals
{
    // The shared popover-surface styling (positioning, overflow, open/close transition) lives in
    // Client/css/components/dropdown-menu.css under .sa-dropdown-menu-content /
    // .sa-dropdown-menu-sub-content.

    /// <summary>Renders a semantic icon (via <see cref="IconTagHelper" />) as inline content.</summary>
    public static IHtmlContent RenderIcon(
        TagHelperContext context,
        IconOptions iconOptions,
        SemanticIconRole role,
        string cssClass
    )
    {
        var iconOutput = new TagHelperOutput(
            string.Empty,
            [new TagHelperAttribute("class", cssClass)],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(iconOptions)
        {
            Name = iconOptions.GetSemanticIconName(role),
        };
        iconTagHelper.Process(context, iconOutput);

        return iconOutput;
    }

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
