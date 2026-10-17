using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a dropdown menu item that can be checked and unchecked.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-checkbox-item")]
public sealed class DropdownMenuCheckboxItemTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is checked.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item is checked; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether clicking the item closes the menu.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if clicking the item closes the menu; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("close-on-click")]
    public bool? CloseOnClick { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item is disabled and can't be chosen; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is inset.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item's text aligns with items that have a leading icon;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("inset")]
    public bool? Inset { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DropdownMenuCheckboxItemTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the check mark.</param>
    public DropdownMenuCheckboxItemTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var isChecked = Checked ?? false;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "menuitemcheckbox");
        output.Attributes.SetAttribute("tabindex", "-1");
        output.Attributes.SetAttribute("aria-checked", isChecked ? "true" : "false");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-checkbox-item");
        output.Attributes.SetAttribute("data-state", isChecked ? "checked" : "unchecked");
        if (Inset == true)
        {
            output.Attributes.SetAttribute("data-inset", "true");
        }

        // Only emit when the author overrides the default (checkbox items stay open on click).
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
            JoinCssClasses("sa-dropdown-menu-checkbox-item", output.GetUserSuppliedClass())
        );

        var indicator = new TagBuilder("span");
        indicator.Attributes["data-slot"] = "dropdown-menu-checkbox-item-indicator";
        indicator.Attributes["class"] = JoinCssClasses("sa-dropdown-menu-item-indicator");
        indicator.InnerHtml.AppendHtml(
            DropdownMenuInternals.RenderIcon(
                context,
                _iconOptions,
                SemanticIconRole.MenuItemSelected,
                "size-4"
            )
        );

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(indicator);
        output.Content.AppendHtml(childContent);
    }
}
