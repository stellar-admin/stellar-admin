using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the main content area beside a sidebar, as a <c>&lt;main&gt;</c> element.
/// </summary>
[HtmlTargetElement("sa-sidebar-inset")]
public class SidebarInsetTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
