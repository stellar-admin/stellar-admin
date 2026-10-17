using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a keyboard shortcut hint at the end of a dropdown menu item.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-shortcut")]
public sealed class DropdownMenuShortcutTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
