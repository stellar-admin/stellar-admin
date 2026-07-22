using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A button rendered as an entry within a sidebar menu item.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-button")]
public class SidebarMenuButtonTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    private static readonly Dictionary<SidebarMenuButtonSize, ThemeToken> SizeClasses = new()
    {
        [SidebarMenuButtonSize.Default] = new ThemeToken("sa-sidebar-menu-button-size-default"),
        [SidebarMenuButtonSize.Small] = new ThemeToken("sa-sidebar-menu-button-size-sm"),
        [SidebarMenuButtonSize.Large] = new ThemeToken("sa-sidebar-menu-button-size-lg"),
    };

    private static readonly Dictionary<SidebarMenuButtonVariant, ThemeToken> VariantClasses = new()
    {
        [SidebarMenuButtonVariant.Default] = new ThemeToken(
            "sa-sidebar-menu-button-variant-default"
        ),
        [SidebarMenuButtonVariant.Outline] = new ThemeToken(
            "sa-sidebar-menu-button-variant-outline"
        ),
    };

    /// <summary>
    ///     The size of the menu button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SidebarMenuButtonSize.Default" />.
    /// </remarks>
    public SidebarMenuButtonSize? Size { get; set; }

    /// <summary>
    ///     The visual variant of the menu button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SidebarMenuButtonVariant.Default" />.
    /// </remarks>
    public SidebarMenuButtonVariant? Variant { get; set; }

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
        if (IsActive ?? false)
        {
            output.Attributes.SetAttribute("data-active", null);
        }

        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-sidebar-menu-button"),
                "peer/menu-button group/menu-button",
                SizeClasses[effectiveSize],
                VariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
