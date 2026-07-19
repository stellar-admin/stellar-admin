using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of the sidebar, pinned above its content; typically holds branding or a workspace switcher.
/// </summary>
[HtmlTargetElement("sa-sidebar-header")]
public class SidebarHeaderTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(themeManager, classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-header");
        output.Attributes.SetAttribute("data-sidebar", "header");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-sidebar-header"),
                "flex flex-col",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
