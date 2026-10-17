using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a horizontal line that divides sections of a sidebar.
/// </summary>
[HtmlTargetElement("sa-sidebar-separator")]
public class SidebarSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
