using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button in a sidebar submenu.
/// </summary>
/// <remarks>
///     The button has <c>type="button"</c> unless you set <c>type</c>.
/// </remarks>
[HtmlTargetElement("sa-sidebar-menu-sub-button")]
public sealed class SidebarMenuSubButtonTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="SidebarMenuSubButtonSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public SidebarMenuSubButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the button is the current item.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the button is highlighted as the current item; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("active")]
    public bool? Active { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? SidebarMenuSubButtonSize.Default;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-sub-button");
        output.Attributes.SetAttribute("data-sidebar", "menu-sub-button");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        if (Active ?? false)
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
