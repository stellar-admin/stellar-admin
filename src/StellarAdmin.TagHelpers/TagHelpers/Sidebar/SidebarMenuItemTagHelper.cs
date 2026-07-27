using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A single item within a sidebar menu, rendered as a list item.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-item")]
public class SidebarMenuItemTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-item");
        output.Attributes.SetAttribute("data-sidebar", "menu-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-menu-item", "group/menu-item", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
