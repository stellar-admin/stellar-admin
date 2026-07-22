using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     An anchor rendered as an entry within a nested sidebar submenu, with routing support; marks itself active when it matches the current route.
/// </summary>
[HtmlTargetElement("sa-sidebar-menu-sub-link")]
public class SidebarMenuSubLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     The size of the submenu link.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SidebarMenuSubLinkSize.Medium" />.
    /// </remarks>
    public SidebarMenuSubLinkSize? Size { get; set; }

    public SidebarMenuSubLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? SidebarMenuSubLinkSize.Medium;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        var anchorTagHelper = new AnchorTagHelper(_htmlGenerator)
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

        output.Attributes.SetAttribute("data-slot", "sidebar-menu-sub-button");
        output.Attributes.SetAttribute("data-sidebar", "menu-sub-button");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        if (IsActiveRoute())
        {
            output.Attributes.SetAttribute("data-active", true);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sidebar-menu-sub-button", output.GetUserSuppliedClass())
        );
    }
}
