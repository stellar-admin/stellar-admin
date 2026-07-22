using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The main content area shown alongside the sidebar, rendered as a <c>&lt;main&gt;</c> element.
/// </summary>
[HtmlTargetElement("sa-sidebar-inset")]
public class SidebarInsetTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "main";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-inset");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-inset", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
