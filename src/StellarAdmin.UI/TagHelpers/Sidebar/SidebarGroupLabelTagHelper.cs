using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the label of a sidebar group.
/// </summary>
/// <remarks>
///     The label is hidden while the sidebar is collapsed to icons.
/// </remarks>
[HtmlTargetElement("sa-sidebar-group-label")]
public sealed class SidebarGroupLabelTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sidebar-group-label");
        output.Attributes.SetAttribute("data-sidebar", "group-label");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-group-label", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
