using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A list of menu items within a sidebar group, rendered as a list.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu")]
public class SidebarMenuTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(themeManager, classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu");
        output.Attributes.SetAttribute("data-sidebar", "menu");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-sidebar-menu"),
                "flex w-full min-w-0 flex-col",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
