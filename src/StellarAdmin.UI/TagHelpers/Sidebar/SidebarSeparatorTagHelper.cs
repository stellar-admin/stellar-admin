using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A horizontal separator used to divide sections of the sidebar.
/// </summary>
[HtmlTargetElement("sa-sidebar-separator")]
public class SidebarSeparatorTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "sidebar-separator");
        output.Attributes.SetAttribute("data-sidebar", "separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-separator", output.GetUserSuppliedClass())
        );

        var separatorTagHelper = new SeparatorTagHelper()
        {
            Orientation = SeparatorOrientation.Horizontal,
        };

        await separatorTagHelper.ProcessAsync(context, output);
    }
}
