using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a submenu in a sidebar menu item, as a list.
/// </summary>
/// <remarks>
///     The submenu is hidden while the sidebar is collapsed to icons.
/// </remarks>
[HtmlTargetElement("sa-sidebar-menu-sub")]
public class SidebarMenuSubTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
