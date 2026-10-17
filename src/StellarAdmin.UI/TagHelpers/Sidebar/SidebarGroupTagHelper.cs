using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a section of a sidebar that groups related menu items.
/// </summary>
[HtmlTargetElement("sa-sidebar-group")]
public sealed class SidebarGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group");
        output.Attributes.SetAttribute("data-sidebar", "group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
