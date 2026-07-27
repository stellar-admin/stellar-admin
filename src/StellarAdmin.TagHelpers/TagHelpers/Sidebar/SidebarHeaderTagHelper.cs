using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The header region of the sidebar, pinned above its content; typically holds branding or a workspace switcher.
/// </summary>
[HtmlTargetElement("sa-sidebar-header")]
public class SidebarHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-header");
        output.Attributes.SetAttribute("data-sidebar", "header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
