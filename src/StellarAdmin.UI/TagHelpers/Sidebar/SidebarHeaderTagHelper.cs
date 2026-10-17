using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of a sidebar, above its content.
/// </summary>
/// <remarks>
///     The header usually holds branding or a workspace switcher.
/// </remarks>
[HtmlTargetElement("sa-sidebar-header")]
public class SidebarHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
