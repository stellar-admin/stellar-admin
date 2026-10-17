using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the footer of a sidebar, below its content.
/// </summary>
/// <remarks>
///     The footer usually holds a user menu or secondary actions.
/// </remarks>
[HtmlTargetElement("sa-sidebar-footer")]
public sealed class SidebarFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
