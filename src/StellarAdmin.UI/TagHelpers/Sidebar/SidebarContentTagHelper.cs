using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The main scrollable content region of the sidebar; holds the sidebar's groups and menus.
/// </summary>
[HtmlTargetElement("sa-sidebar-content")]
public class SidebarContentTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-content");
        output.Attributes.SetAttribute("data-sidebar", "content");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-sidebar-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
