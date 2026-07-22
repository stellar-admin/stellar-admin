using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A single item within a nested sidebar submenu, rendered as a list item.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-sub-item")]
public class SidebarMenuSubItemTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-sub-item");
        output.Attributes.SetAttribute("data-sidebar", "menu-sub-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-sidebar-menu-sub-item",
                "group/menu-sub-item",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
