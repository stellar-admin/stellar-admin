using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button in a sidebar menu item.
/// </summary>
/// <remarks>
///     The button has <c>type="button"</c> unless you set <c>type</c>.
/// </remarks>
[HtmlTargetElement("sa-sidebar-menu-button")]
public class SidebarMenuButtonTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<SidebarMenuButtonSize, string> SizeClasses = new()
    {
        [SidebarMenuButtonSize.Default] = "sa-sidebar-menu-button-size-default",
        [SidebarMenuButtonSize.Small] = "sa-sidebar-menu-button-size-sm",
        [SidebarMenuButtonSize.Large] = "sa-sidebar-menu-button-size-lg",
    };

    private static readonly Dictionary<SidebarMenuButtonVariant, string> VariantClasses = new()
    {
        [SidebarMenuButtonVariant.Default] = "sa-sidebar-menu-button-variant-default",
        [SidebarMenuButtonVariant.Outline] = "sa-sidebar-menu-button-variant-outline",
    };

    /// <summary>
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="SidebarMenuButtonSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public SidebarMenuButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="SidebarMenuButtonVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public SidebarMenuButtonVariant? Variant { get; set; }

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
        var effectiveSize = Size ?? SidebarMenuButtonSize.Default;
        var effectiveVariant = Variant ?? SidebarMenuButtonVariant.Default;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-button");
        output.Attributes.SetAttribute("data-sidebar", "menu-button");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        if (Active ?? false)
        {
            output.Attributes.SetAttribute("data-active", null);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-sidebar-menu-button",
                SizeClasses[effectiveSize],
                VariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
