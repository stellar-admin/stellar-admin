using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Displays a keyboard shortcut hint, aligned to the trailing edge of a menu item.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-shortcut")]
public class DropdownMenuShortcutTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dropdown-menu-shortcut");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dropdown-menu-shortcut", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
