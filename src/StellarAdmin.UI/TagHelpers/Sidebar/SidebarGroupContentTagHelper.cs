using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The content region of a sidebar group, wrapping the group's menu.
/// </summary>
[HtmlTargetElement("sa-sidebar-group-content")]
public class SidebarGroupContentTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group-content");
        output.Attributes.SetAttribute("data-sidebar", "group-content");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-sidebar-group-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
