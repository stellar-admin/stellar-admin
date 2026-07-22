using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The label heading for a sidebar group.
/// </summary>
[HtmlTargetElement("sa-sidebar-group-label")]
public class SidebarGroupLabelTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group-label");
        output.Attributes.SetAttribute("data-sidebar", "group-label");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-group-label", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
