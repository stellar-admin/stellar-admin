using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the dropdown menu item that opens a submenu.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-sub-trigger")]
public class DropdownMenuSubTriggerTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets a value that indicates whether the trigger is inset.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the trigger's text aligns with items that have a leading
    ///     icon; otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("inset")]
    public bool? Inset { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DropdownMenuSubTriggerTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the submenu arrow.</param>
    public DropdownMenuSubTriggerTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "menuitem");
        output.Attributes.SetAttribute("tabindex", "-1");
        output.Attributes.SetAttribute("aria-haspopup", "menu");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-sub-trigger");

        var subId = GetContext<DropdownMenuContext>(context)?.MenuId;
        if (subId != null)
        {
            output.Attributes.SetAttribute("popovertarget", subId);
            output.Attributes.SetAttribute("interestfor", subId);

            // The sub-trigger is a <div>, so `popovertarget` doesn't establish the implicit
            // CSS anchor the way a <button> trigger does. Name it explicitly (reusing SubId,
            // a valid dashed-ident) so the sub-content can position against it.
            output.AppendStyle($"anchor-name: {subId}");
        }

        if (Inset == true)
        {
            output.Attributes.SetAttribute("data-inset", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dropdown-menu-sub-trigger", output.GetUserSuppliedClass())
        );

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(childContent);
        output.Content.AppendHtml(
            DropdownMenuInternals.RenderIcon(
                context,
                _iconOptions,
                SemanticIconRole.SubmenuIndicator,
                "size-4"
            )
        );
    }
}
