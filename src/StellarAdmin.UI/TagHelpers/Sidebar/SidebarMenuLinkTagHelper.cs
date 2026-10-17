using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a link in a sidebar menu item.
/// </summary>
/// <remarks>
///     The link is highlighted as the current item when its page or action routing attributes
///     match the current request.
/// </remarks>
[HtmlTargetElement("sa-sidebar-menu-link")]
public sealed class SidebarMenuLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private static readonly Dictionary<SidebarMenuLinkSize, string> SizeClasses = new()
    {
        [SidebarMenuLinkSize.Default] = "sa-sidebar-menu-button-size-default",
        [SidebarMenuLinkSize.Small] = "sa-sidebar-menu-button-size-sm",
        [SidebarMenuLinkSize.Large] = "sa-sidebar-menu-button-size-lg",
    };

    private static readonly Dictionary<SidebarMenuLinkVariant, string> VariantClasses = new()
    {
        [SidebarMenuLinkVariant.Default] = "sa-sidebar-menu-button-variant-default",
        [SidebarMenuLinkVariant.Outline] = "sa-sidebar-menu-button-variant-outline",
    };

    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Gets or sets the size of the link.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="SidebarMenuLinkSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public SidebarMenuLinkSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the link.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="SidebarMenuLinkVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public SidebarMenuLinkVariant? Variant { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SidebarMenuLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that resolves the link's <c>href</c> from its routing attributes.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public SidebarMenuLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? SidebarMenuLinkSize.Default;
        var effectiveVariant = Variant ?? SidebarMenuLinkVariant.Default;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-button");
        output.Attributes.SetAttribute("data-sidebar", "menu-button");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        if (IsActiveRoute())
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
    }
}
