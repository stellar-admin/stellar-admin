using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkAnchorTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An anchor rendered as an entry within a sidebar menu item, with routing support; marks itself active when it matches the current route.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-link")]
public class SidebarMenuLinkTagHelper : StellarAdminAnchorTagHelperBase
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
    ///     The size of the menu link.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SidebarMenuLinkSize.Default" />.
    /// </remarks>
    public SidebarMenuLinkSize? Size { get; set; }

    /// <summary>
    ///     The visual variant of the menu link.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SidebarMenuLinkVariant.Default" />.
    /// </remarks>
    public SidebarMenuLinkVariant? Variant { get; set; }

    public SidebarMenuLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? SidebarMenuLinkSize.Default;
        var effectiveVariant = Variant ?? SidebarMenuLinkVariant.Default;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        var anchorTagHelper = new FrameworkAnchorTagHelper(_htmlGenerator)
        {
            ViewContext = ViewContext,
            Action = Action,
            Area = Area,
            Controller = Controller,
            Fragment = Fragment,
            Host = Host,
            Page = Page,
            PageHandler = PageHandler,
            Protocol = Protocol,
            Route = Route,
            RouteValues = RouteValues,
        };
        await anchorTagHelper.ProcessAsync(context, output);

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
                "peer/menu-button group/menu-button",
                SizeClasses[effectiveSize],
                VariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );
    }
}
