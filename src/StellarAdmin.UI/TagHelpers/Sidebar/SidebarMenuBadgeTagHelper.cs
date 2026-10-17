using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a small badge, such as a count, at the end of a sidebar menu item.
/// </summary>
/// <remarks>
///     The badge is hidden while the sidebar is collapsed to icons.
/// </remarks>
[HtmlTargetElement("sa-sidebar-menu-badge")]
public sealed class SidebarMenuBadgeTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
