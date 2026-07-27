using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The footer region of the sidebar, pinned below its content; typically holds a user menu or secondary actions.
/// </summary>
[HtmlTargetElement("sa-sidebar-footer")]
public class SidebarFooterTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-footer");
        output.Attributes.SetAttribute("data-sidebar", "footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
