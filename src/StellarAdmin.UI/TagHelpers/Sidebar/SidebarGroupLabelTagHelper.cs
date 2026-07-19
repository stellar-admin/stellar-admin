using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The label heading for a sidebar group.
/// </summary>
[HtmlTargetElement("sa-sidebar-group-label")]
public class SidebarGroupLabelTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(themeManager, classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group-label");
        output.Attributes.SetAttribute("data-sidebar", "group-label");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-sidebar-group-label"),
                "flex shrink-0 items-center outline-hidden [&>svg]:shrink-0",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
