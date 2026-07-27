using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A horizontal divider that visually separates groups of menu items.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-separator")]
public class DropdownMenuSeparatorTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute("aria-orientation", "horizontal");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dropdown-menu-separator", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
