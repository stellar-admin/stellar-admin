using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Displays a keyboard shortcut hint, aligned to the trailing edge of a menu item.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-shortcut")]
public class DropdownMenuShortcutTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dropdown-menu-shortcut");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-dropdown-menu-shortcut", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
