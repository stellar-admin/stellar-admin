using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

internal static class CommandRenderingHelper
{
    public static void RenderItem(
        TagHelperOutput output,
        IconOptions iconOptions,
        string? value,
        string? keywords,
        bool? disabled,
        bool? isChecked
    )
    {
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "option");
        output.Attributes.SetAttribute("aria-selected", "false");
        output.Attributes.SetAttribute("tabindex", "-1");
        output.Attributes.SetAttribute("data-slot", "command-item");
        if (!string.IsNullOrEmpty(value))
        {
            output.Attributes.SetAttribute("data-value", value);
        }

        if (!string.IsNullOrEmpty(keywords))
        {
            output.Attributes.SetAttribute("data-keywords", keywords);
        }

        if (disabled == true)
        {
            output.Attributes.SetAttribute("data-disabled", "true");
            output.Attributes.SetAttribute("aria-disabled", "true");
        }

        if (isChecked == true)
        {
            output.Attributes.SetAttribute("data-checked", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-command-item",
                output.GetUserSuppliedClass()
            )
        );

        output.PostContent.AppendHtml(
            iconOptions.RenderIcon(
                SemanticIconRole.MenuItemSelected,
                ("class", "sa-command-item-indicator")
            )
        );
    }
}
