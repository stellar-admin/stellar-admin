using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the scrollable content of a sidebar.
/// </summary>
/// <remarks>
///     The content holds the sidebar's groups and menus.
/// </remarks>
[HtmlTargetElement("sa-sidebar-content")]
public class SidebarContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-content");
        output.Attributes.SetAttribute("data-sidebar", "content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
