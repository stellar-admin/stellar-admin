using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

internal static class CommandRenderingHelper
{
    public static void RenderItem(
        TagHelperContext context,
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
            RenderIcon(
                context,
                iconOptions,
                SemanticIconRole.MenuItemSelected,
                "sa-command-item-indicator"
            )
        );
    }

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
}
