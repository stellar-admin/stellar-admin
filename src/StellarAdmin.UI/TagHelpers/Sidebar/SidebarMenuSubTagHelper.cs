using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A nested submenu within a sidebar menu item, rendered as a list.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-sub")]
public class SidebarMenuSubTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-sub");
        output.Attributes.SetAttribute("data-sidebar", "menu-sub");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-menu-sub", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
