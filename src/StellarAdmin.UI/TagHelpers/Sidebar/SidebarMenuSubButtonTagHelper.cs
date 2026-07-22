using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A button rendered as an entry within a nested sidebar submenu.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-sub-button")]
public class SidebarMenuSubButtonTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The size of the submenu button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SidebarMenuSubLinkSize.Medium" />.
    /// </remarks>
    public SidebarMenuSubLinkSize? Size { get; set; }

    /// <summary>
    ///     Whether this button represents the active item; when <c>true</c> the button is
    ///     marked with a <c>data-active</c> attribute for styling.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public bool? IsActive { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? SidebarMenuSubLinkSize.Medium;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-sub-button");
        output.Attributes.SetAttribute("data-sidebar", "menu-sub-button");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        if (IsActive ?? false)
        {
            output.Attributes.SetAttribute("data-active", true);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-menu-sub-button", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
