using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A horizontal separator used to divide sections of the sidebar.
/// </summary>
[HtmlTargetElement("sa-sidebar-separator")]
public class SidebarSeparatorTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(themeManager, classMerger)
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "sidebar-separator");
        output.Attributes.SetAttribute("data-sidebar", "separator");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-sidebar-separator"),
                "w-auto",
                output.GetUserSuppliedClass()
            )
        );

        var separatorTagHelper = new SeparatorTagHelper(ThemeManager, ClassMerger)
        {
            Orientation = SeparatorOrientation.Horizontal,
        };

        await separatorTagHelper.ProcessAsync(context, output);
    }
}
