using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a line between groups of dropdown menu items.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-separator")]
public sealed class DropdownMenuSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
