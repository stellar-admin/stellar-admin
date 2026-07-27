using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A titled section within the sidebar that groups related menu items together.
/// </summary>
[HtmlTargetElement("sa-sidebar-group")]
public class SidebarGroupTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group");
        output.Attributes.SetAttribute("data-sidebar", "group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
