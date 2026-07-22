using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A small badge, typically a count, shown at the end of a sidebar menu item; hidden while the sidebar is collapsed to icons.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-badge")]
public class SidebarMenuBadge : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-badge");
        output.Attributes.SetAttribute("data-sidebar", "menu-badge");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-menu-badge", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
