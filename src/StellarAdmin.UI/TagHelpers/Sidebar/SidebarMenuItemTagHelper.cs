using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an item in a sidebar menu, as a list item.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-item")]
public sealed class SidebarMenuItemTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-item");
        output.Attributes.SetAttribute("data-sidebar", "menu-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-menu-item", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
