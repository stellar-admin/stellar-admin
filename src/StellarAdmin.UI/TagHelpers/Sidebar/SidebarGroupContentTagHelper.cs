using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the content of a sidebar group, which holds the group's menu.
/// </summary>
[HtmlTargetElement("sa-sidebar-group-content")]
public class SidebarGroupContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group-content");
        output.Attributes.SetAttribute("data-sidebar", "group-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-group-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
