using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A list of menu items within a sidebar group, rendered as a list.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu")]
public class SidebarMenuTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-menu");
        output.Attributes.SetAttribute("data-sidebar", "menu");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-sidebar-menu", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
