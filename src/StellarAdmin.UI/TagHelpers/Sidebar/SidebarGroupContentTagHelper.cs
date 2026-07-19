using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The content region of a sidebar group, wrapping the group's menu.
/// </summary>
[HtmlTargetElement("sa-sidebar-group-content")]
public class SidebarGroupContentTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(themeManager, classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group-content");
        output.Attributes.SetAttribute("data-sidebar", "group-content");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-sidebar-group-content"),
                "w-full",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
