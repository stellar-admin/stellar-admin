using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a menu in a sidebar, as a list of menu items.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu")]
public class SidebarMenuTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu");
        output.Attributes.SetAttribute("data-sidebar", "menu");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-menu", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
