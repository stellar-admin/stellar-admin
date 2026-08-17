using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A selectable menu item. Renders as a
///     <c>&lt;div role="menuitem"&gt;</c>, or as an <c>&lt;a role="menuitem"&gt;</c> when the author
///     supplies a URL — either a raw <c>href</c> or ASP.NET routing attributes (<c>asp-page</c>,
///     <c>asp-action</c>/<c>asp-controller</c>, <c>asp-route-*</c>, …). The <c>sel-dropdown-menu</c>
///     web component activates items by <c>role</c>, so both elements behave identically.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-item")]
public class DropdownMenuItemTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public DropdownMenuItemTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Whether clicking the item closes the menu. Plain items close on click unless this is set.
    /// </summary>
    [HtmlAttributeName("close-on-click")]
    public bool? CloseOnClick { get; set; }

    /// <summary>
    ///     Whether the item is disabled.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     A URL that turns the item into a link. Ignored when ASP.NET routing attributes are
    ///     supplied instead.
    /// </summary>
    [HtmlAttributeName("href")]
    public string? Href { get; set; }

    /// <summary>
    ///     Whether the item is inset, aligning its text with items that have a leading icon.
    /// </summary>
    [HtmlAttributeName("inset")]
    public bool? Inset { get; set; }

    /// <summary>
    ///     The visual style of the item.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DropdownMenuItemVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public DropdownMenuItemVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? DropdownMenuItemVariant.Default;

        // A routing target makes the item a link the framework resolves; a raw href does too.
        var isLink = Href != null || HasRouteTarget;

        output.TagName = isLink ? "a" : "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (HasRouteTarget)
        {
            await ApplyRouteAttributesAsync(_htmlGenerator, context, output);
        }
        else if (Href != null)
        {
            output.Attributes.SetAttribute("href", Href);
        }

        output.Attributes.SetAttribute("role", "menuitem");
        output.Attributes.SetAttribute("tabindex", "-1");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-item");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        if (Inset == true)
        {
            output.Attributes.SetAttribute("data-inset", "true");
        }

        // Only emit when the author overrides the default (plain items close on click).
        if (CloseOnClick.HasValue)
        {
            output.Attributes.SetAttribute(
                "data-close-on-click",
                CloseOnClick.Value ? "true" : "false"
            );
        }

        if (Disabled == true)
        {
            output.Attributes.SetAttribute("data-disabled", "");
            output.Attributes.SetAttribute("aria-disabled", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-dropdown-menu-item",
                "group/dropdown-menu-item",
                output.GetUserSuppliedClass()
            )
        );
    }
}
